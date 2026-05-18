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

window.schoolGrowthTables = (() => {
    const instances = new Map();

    function attach(selector) {
        document.querySelectorAll(selector).forEach(wrap => {
            const table = wrap.querySelector(":scope > table");
            const thead = table?.querySelector("thead");
            if (!table || !thead) return;

            let instance = instances.get(wrap);
            if (!instance) {
                const floating = document.createElement("div");
                floating.className = "floating-table-head";

                const floatingTable = table.cloneNode(false);
                floatingTable.appendChild(thead.cloneNode(true));
                floating.appendChild(floatingTable);
                document.body.appendChild(floating);

                instance = {
                    wrap,
                    table,
                    thead,
                    floating,
                    floatingTable,
                    resizeObserver: new ResizeObserver(() => update(wrap))
                };
                instances.set(wrap, instance);

                wrap.addEventListener("scroll", () => update(wrap), { passive: true });
                instance.resizeObserver.observe(wrap);
                instance.resizeObserver.observe(table);
            } else {
                instance.table = table;
                instance.thead = thead;
                instance.floatingTable.replaceChildren(thead.cloneNode(true));
            }

            wireFloatingInputs(instance);
            update(wrap);
        });

        if (!window.__schoolGrowthTableHeadersAttached) {
            window.__schoolGrowthTableHeadersAttached = true;
            window.addEventListener("scroll", updateAll, { passive: true });
            window.addEventListener("resize", updateAll, { passive: true });
        }
    }

    function updateAll() {
        for (const wrap of instances.keys()) {
            update(wrap);
        }
    }

    function update(wrap) {
        const instance = instances.get(wrap);
        if (!instance || !document.body.contains(wrap)) return;

        const tableRect = instance.table.getBoundingClientRect();
        const wrapRect = wrap.getBoundingClientRect();
        const headerHeight = instance.thead.getBoundingClientRect().height || 0;
        const visible = tableRect.top < 0
            && tableRect.bottom > headerHeight
            && wrapRect.bottom > headerHeight
            && wrapRect.right > 0
            && wrapRect.left < window.innerWidth;

        if (!visible) {
            instance.floating.style.display = "none";
            return;
        }

        syncWidths(instance);
        instance.floating.style.display = "block";
        instance.floating.style.left = `${Math.max(0, wrapRect.left)}px`;
        instance.floating.style.width = `${Math.min(wrapRect.width, window.innerWidth - Math.max(0, wrapRect.left))}px`;
        instance.floating.style.setProperty("--table-scroll-left", `${wrap.scrollLeft}px`);
        instance.floatingTable.style.transform = `translateX(${-wrap.scrollLeft}px)`;
    }

    function syncWidths(instance) {
        const sourceCells = instance.thead.querySelectorAll("th");
        const targetCells = instance.floatingTable.querySelectorAll("th");
        const tableWidth = instance.table.getBoundingClientRect().width;

        instance.floatingTable.style.width = `${tableWidth}px`;
        for (let index = 0; index < sourceCells.length && index < targetCells.length; index++) {
            const width = sourceCells[index].getBoundingClientRect().width;
            targetCells[index].style.width = `${width}px`;
            targetCells[index].style.minWidth = `${width}px`;
            targetCells[index].style.maxWidth = `${width}px`;
        }

        const sourceInputs = instance.thead.querySelectorAll("input");
        const targetInputs = instance.floatingTable.querySelectorAll("input");
        for (let index = 0; index < sourceInputs.length && index < targetInputs.length; index++) {
            targetInputs[index].checked = sourceInputs[index].checked;
        }
    }

    function wireFloatingInputs(instance) {
        const sourceInputs = instance.thead.querySelectorAll("input");
        const targetInputs = instance.floatingTable.querySelectorAll("input");
        for (let index = 0; index < sourceInputs.length && index < targetInputs.length; index++) {
            targetInputs[index].addEventListener("change", () => {
                sourceInputs[index].checked = targetInputs[index].checked;
                sourceInputs[index].dispatchEvent(new Event("change", { bubbles: true }));
                window.setTimeout(() => update(instance.wrap), 0);
            });
        }
    }

    return { attach, updateAll };
})();
