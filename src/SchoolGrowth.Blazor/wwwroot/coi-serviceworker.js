(function () {
    const readyEventName = "schoolgrowth-coi-ready";
    const reloadKey = "schoolGrowthCoiReloadCount";
    const reloadParameter = "sg-coi-reload";

    const isolationHeaders = {
        "Cross-Origin-Opener-Policy": "same-origin",
        "Cross-Origin-Embedder-Policy": "require-corp",
        "Cross-Origin-Resource-Policy": "same-origin"
    };

    const isServiceWorker =
        typeof ServiceWorkerGlobalScope !== "undefined" &&
        self instanceof ServiceWorkerGlobalScope;

    if (isServiceWorker) {
        self.addEventListener("install", event => {
            event.waitUntil(self.skipWaiting());
        });

        self.addEventListener("activate", event => {
            event.waitUntil(self.clients.claim());
        });

        self.addEventListener("fetch", event => {
            if (event.request.cache === "only-if-cached" && event.request.mode !== "same-origin") {
                return;
            }

            event.respondWith((async () => {
                const requestUrl = new URL(event.request.url);
                const shouldBypassCache =
                    requestUrl.origin === self.location.origin &&
                    (requestUrl.pathname.includes("/_framework/") ||
                        requestUrl.pathname.includes("/data/") ||
                        requestUrl.pathname.includes("/sample-data/") ||
                        requestUrl.pathname.endsWith("/coi-serviceworker.js") ||
                        requestUrl.pathname.endsWith("/service-worker-assets.js"));

                const response = await fetch(event.request, shouldBypassCache ? { cache: "no-store" } : undefined);
                if (response.type === "opaque") {
                    return response;
                }

                const headers = new Headers(response.headers);
                for (const [name, value] of Object.entries(isolationHeaders)) {
                    headers.set(name, value);
                }

                return new Response(response.body, {
                    status: response.status,
                    statusText: response.statusText,
                    headers
                });
            })());
        });

        return;
    }

    const markReady = () => {
        sessionStorage.removeItem(reloadKey);
        window.schoolGrowthCoiReady = true;
        window.dispatchEvent(new Event(readyEventName));
    };

    const reloadState = () => {
        try {
            return JSON.parse(sessionStorage.getItem(reloadKey) || "null");
        } catch {
            return null;
        }
    };

    const setReloadState = state => {
        sessionStorage.setItem(reloadKey, JSON.stringify(state));
    };

    const normalizedHref = () => {
        const url = new URL(location.href);
        url.searchParams.delete(reloadParameter);
        return url.href;
    };

    window.schoolGrowthCoiReady = false;

    if (window.crossOriginIsolated) {
        markReady();
        return;
    }

    const canRegister =
        "serviceWorker" in navigator &&
        (location.protocol === "https:" || location.hostname === "localhost" || location.hostname === "127.0.0.1");

    if (!canRegister) {
        console.warn("Cross-origin isolation service worker cannot be registered in this browser/context.");
        return;
    }

    const serviceWorkerUrl = new URL("coi-serviceworker.js", document.baseURI);
    const scope = new URL(".", document.baseURI).href;

    let reloading = false;
    const reloadThroughServiceWorker = () => {
        if (window.crossOriginIsolated) {
            markReady();
            return;
        }

        const now = Date.now();
        const state = reloadState();
        const currentHref = normalizedHref();
        const isFreshState =
            state &&
            state.href === currentHref &&
            now - state.startedAt < 10000;
        const reloadCount = isFreshState ? state.count : 0;

        if (reloading || reloadCount >= 3) {
            const app = document.getElementById("app");
            if (app) {
                app.textContent = "Unable to enable browser thread isolation. Try a normal refresh, or clear this site's service worker/cache.";
            }
            console.warn("Cross-origin isolation service worker registered, but the page did not become isolated.");
            return;
        }

        reloading = true;
        setReloadState({
            href: currentHref,
            startedAt: isFreshState ? state.startedAt : now,
            count: reloadCount + 1
        });

        const nextUrl = new URL(location.href);
        nextUrl.searchParams.set(reloadParameter, String(now));
        location.replace(nextUrl);
    };

    navigator.serviceWorker.addEventListener("controllerchange", reloadThroughServiceWorker);

    navigator.serviceWorker
        .register(serviceWorkerUrl, { scope })
        .then(() => navigator.serviceWorker.ready)
        .then(reloadThroughServiceWorker)
        .catch(error => {
            console.warn("Cross-origin isolation service worker registration failed.", error);
        });
})();
