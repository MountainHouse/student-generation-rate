window.schoolGrowthMasonry = (() => {
    const layouts = new Map();
    const debounceMs = 100;

    function resize(container) {
        if (!container) return;

        const styles = window.getComputedStyle(container);
        const rowHeight = parseFloat(styles.getPropertyValue("grid-auto-rows")) || 8;
        const rowGap = parseFloat(styles.getPropertyValue("row-gap")) || 0;
        const items = Array.from(container.querySelectorAll(".settings-subpanel"));

        for (const item of items) {
            item.style.gridRowEnd = "auto";
        }

        window.requestAnimationFrame(() => {
            for (const item of items) {
                const height = item.getBoundingClientRect().height;
                const span = Math.max(1, Math.ceil((height + rowGap) / (rowHeight + rowGap)));
                item.style.gridRowEnd = `span ${span}`;
            }
        });
    }

    function schedule(container, immediate = false) {
        const layout = layouts.get(container);
        if (!layout) {
            resize(container);
            return;
        }

        window.clearTimeout(layout.timeoutId);
        if (immediate) {
            resize(container);
            return;
        }

        layout.timeoutId = window.setTimeout(() => resize(container), debounceMs);
    }

    function attach(selector) {
        const container = document.querySelector(selector);
        if (!container) return;
        container.classList.add("masonry-ready");

        if (layouts.has(container)) {
            schedule(container, true);
            return;
        }

        const layout = { observer: null, timeoutId: 0 };
        layouts.set(container, layout);

        const observer = new ResizeObserver(() => schedule(container));
        layout.observer = observer;
        observer.observe(container);
        for (const item of container.querySelectorAll(".settings-subpanel")) {
            observer.observe(item);
        }

        container.addEventListener("toggle", event => {
            if (event.target?.classList?.contains("settings-subpanel")) {
                schedule(container, true);
            }
        }, true);

        schedule(container, true);
    }

    return { attach };
})();
