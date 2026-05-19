window.schoolGrowthMasonry = (() => {
    const layouts = new Map();
    const debounceMs = 100;

    function resize(container) {
        if (!container) return;

        const styles = window.getComputedStyle(container);
        const rowHeight = parseFloat(styles.getPropertyValue("grid-auto-rows")) || 8;
        const rowGap = parseFloat(styles.getPropertyValue("row-gap")) || 0;
        const items = Array.from(container.querySelectorAll(".settings-subpanel"));

        window.requestAnimationFrame(() => {
            for (const item of items) {
                const height = item.getBoundingClientRect().height;
                const span = Math.max(1, Math.ceil((height + rowGap) / (rowHeight + rowGap)));
                const next = `span ${span}`;
                if (item.style.gridRowEnd !== next) {
                    item.style.gridRowEnd = next;
                }
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
                    resizeObserver: new ResizeObserver(() => updateAll())
                };
                instances.set(wrap, instance);

                wrap.addEventListener("scroll", updateAll, { passive: true });
                instance.resizeObserver.observe(wrap);
                instance.resizeObserver.observe(table);
            } else {
                instance.table = table;
                instance.thead = thead;
                instance.floatingTable.replaceChildren(thead.cloneNode(true));
            }

            wireFloatingInputs(instance);
            updateAll();
        });

        if (!window.__schoolGrowthTableHeadersAttached) {
            window.__schoolGrowthTableHeadersAttached = true;
            window.addEventListener("scroll", updateAll, { passive: true });
            window.addEventListener("resize", updateAll, { passive: true });
        }
    }

    function updateAll() {
        let active = null;
        let activeTop = -Number.MAX_VALUE;
        for (const [wrap, instance] of instances) {
            const tableRect = instance.table.getBoundingClientRect();
            const wrapRect = wrap.getBoundingClientRect();
            const headerHeight = instance.thead.getBoundingClientRect().height || 0;
            const visible = isFloatingHeaderVisible(tableRect, wrapRect, headerHeight);
            if (visible && tableRect.top > activeTop) {
                active = wrap;
                activeTop = tableRect.top;
            }
        }

        for (const [wrap, instance] of instances) {
            if (wrap === active) {
                update(wrap, true);
            } else {
                instance.floating.style.display = "none";
            }
        }
    }

    function update(wrap, allowVisible = false) {
        const instance = instances.get(wrap);
        if (!instance || !document.body.contains(wrap)) return;

        const tableRect = instance.table.getBoundingClientRect();
        const wrapRect = wrap.getBoundingClientRect();
        const headerHeight = instance.thead.getBoundingClientRect().height || 0;
        const visible = allowVisible && isFloatingHeaderVisible(tableRect, wrapRect, headerHeight);

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

    function isFloatingHeaderVisible(tableRect, wrapRect, headerHeight) {
        return tableRect.top < 0
            && tableRect.bottom > headerHeight
            && wrapRect.bottom > headerHeight
            && wrapRect.right > 0
            && wrapRect.left < window.innerWidth;
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

window.schoolGrowthTools = (() => {
    const dragState = new WeakMap();

    function scrollToTool(id) {
        const element = document.getElementById(`tool-${id}`);
        if (!element) return;

        element.scrollIntoView({ behavior: "smooth", block: "start", inline: "nearest" });
    }

    function attachDraggable(selector) {
        document.querySelectorAll(selector).forEach(element => {
            if (dragState.has(element)) return;

            const button = element.querySelector(".tool-nav-toggle");
            if (!button) return;

            const state = {
                dragging: false,
                moved: false,
                pointerId: null,
                offsetX: 0,
                offsetY: 0,
                startX: 0,
                startY: 0
            };
            dragState.set(element, state);

            button.addEventListener("pointerdown", event => {
                if (event.button !== undefined && event.button !== 0) return;

                const rect = element.getBoundingClientRect();
                state.dragging = true;
                state.moved = false;
                state.pointerId = event.pointerId;
                state.offsetX = event.clientX - rect.left;
                state.offsetY = event.clientY - rect.top;
                state.startX = event.clientX;
                state.startY = event.clientY;
                button.setPointerCapture?.(event.pointerId);
            });

            button.addEventListener("pointermove", event => {
                if (!state.dragging || state.pointerId !== event.pointerId) return;

                const deltaX = Math.abs(event.clientX - state.startX);
                const deltaY = Math.abs(event.clientY - state.startY);
                if (deltaX + deltaY > 4) {
                    state.moved = true;
                }

                const rect = element.getBoundingClientRect();
                const left = Math.min(
                    Math.max(8, event.clientX - state.offsetX),
                    window.innerWidth - rect.width - 8);
                const top = Math.min(
                    Math.max(8, event.clientY - state.offsetY),
                    window.innerHeight - rect.height - 8);

                element.style.left = `${left}px`;
                element.style.top = `${top}px`;
                element.style.right = "auto";
                element.style.bottom = "auto";
            });

            button.addEventListener("pointerup", event => {
                if (!state.dragging || state.pointerId !== event.pointerId) return;

                state.dragging = false;
                state.pointerId = null;
                button.releasePointerCapture?.(event.pointerId);
            });

            button.addEventListener("click", event => {
                if (!state.moved) return;

                event.preventDefault();
                event.stopImmediatePropagation();
                state.moved = false;
            }, true);
        });
    }

    function setOpen(selector, open) {
        document.querySelectorAll(selector).forEach(element => {
            if (!open) {
                element.classList.remove("tool-nav-open-above");
                element.classList.remove("tool-nav-open-right");
                const popup = element.querySelector(".tool-nav-popup");
                if (popup) {
                    popup.style.maxHeight = "";
                }
                return;
            }

            window.requestAnimationFrame(() => {
                chooseOpenDirection(element);
                clampVerticallyIntoViewport(element);
            });
        });
    }

    function chooseOpenDirection(element) {
        const header = element.querySelector(".tool-nav-header");
        const popup = element.querySelector(".tool-nav-popup");
        if (!header || !popup) return;

        element.classList.remove("tool-nav-open-above");
        element.classList.remove("tool-nav-open-right");
        const headerRect = header.getBoundingClientRect();
        const popupRect = popup.getBoundingClientRect();
        const popupHeight = popupRect.height;
        const popupWidth = popupRect.width;
        const margin = 8;
        const spaceBelow = window.innerHeight - headerRect.bottom - margin;
        const spaceAbove = headerRect.top - margin;
        const spaceLeft = headerRect.right - margin;
        const spaceRight = window.innerWidth - headerRect.left - margin;

        if (spaceBelow < popupHeight && spaceAbove > spaceBelow) {
            element.classList.add("tool-nav-open-above");
        }

        if (spaceLeft < popupWidth && spaceRight > spaceLeft) {
            element.classList.add("tool-nav-open-right");
        }
    }

    function clampVerticallyIntoViewport(element) {
        const panel = element.querySelector(".tool-nav-panel");
        if (!panel) return;

        const header = panel.querySelector(".tool-nav-header");
        const popup = panel.querySelector(".tool-nav-popup");
        if (!header) return;

        const headerRect = header.getBoundingClientRect();
        const popupRect = popup?.getBoundingClientRect();

        let left = element.style.left ? parseFloat(element.style.left) : element.getBoundingClientRect().left;
        let top = element.style.top ? parseFloat(element.style.top) : element.getBoundingClientRect().top;
        const margin = 8;
        const opensAbove = element.classList.contains("tool-nav-open-above");

        if (opensAbove) {
            if (headerRect.bottom > window.innerHeight - margin) {
                top -= headerRect.bottom - (window.innerHeight - margin);
            }
            if (headerRect.top < margin && (!popupRect || popupRect.top >= margin)) {
                top += margin - headerRect.top;
            }
            if (popup && popupRect && popupRect.top < margin) {
                const maxPopupHeight = Math.max(120, headerRect.top - margin);
                popup.style.maxHeight = `${Math.min(maxPopupHeight, window.innerHeight - margin * 2)}px`;
            }
        } else {
            if (headerRect.top < margin) {
                top += margin - headerRect.top;
            }
            if (popupRect && popupRect.bottom > window.innerHeight - margin) {
                top -= popupRect.bottom - (window.innerHeight - margin);
            }
        }

        const headerWidth = headerRect.width || 120;
        const headerHeight = headerRect.height || 34;
        left = Math.max(margin, Math.min(left, window.innerWidth - Math.min(headerWidth, window.innerWidth - margin * 2) - margin));
        top = Math.max(margin, Math.min(top, window.innerHeight - Math.min(headerHeight, window.innerHeight - margin * 2) - margin));

        element.style.left = `${left}px`;
        element.style.top = `${top}px`;
        element.style.right = "auto";
        element.style.bottom = "auto";
    }

    return { scrollToTool, attachDraggable, setOpen };
})();

window.schoolGrowthStorage = (() => {
    function getJson(key) {
        try {
            const value = window.localStorage.getItem(key);
            return value ? JSON.parse(value) : null;
        } catch {
            return null;
        }
    }

    function setJson(key, value) {
        try {
            window.localStorage.setItem(key, JSON.stringify(value));
        } catch {
            // Local storage can be unavailable in private or restricted browser modes.
        }
    }

    function remove(key) {
        try {
            window.localStorage.removeItem(key);
        } catch {
        }
    }

    function removePrefix(prefix) {
        try {
            const keys = [];
            for (let index = 0; index < window.localStorage.length; index++) {
                const key = window.localStorage.key(index);
                if (key?.startsWith(prefix)) {
                    keys.push(key);
                }
            }
            for (const key of keys) {
                window.localStorage.removeItem(key);
            }
        } catch {
        }
    }

    return { getJson, setJson, remove, removePrefix };
})();

window.schoolGrowthValidationChart = (() => {
    function attach(element, dotNet, pointCount, chartId = "validation-summary") {
        if (!element || !dotNet || !pointCount) return;

        const state = element.__schoolGrowthValidationHover ?? {};
        state.dotNet = dotNet;
        state.pointCount = pointCount;
        state.chartId = chartId;
        state.lastIndex = Number.isInteger(state.lastIndex) ? state.lastIndex : -2;

        if (!state.attached) {
            state.onPointerMove = event => {
                const current = element.__schoolGrowthValidationHover;
                if (!current?.dotNet || !current.pointCount) return;

                const rect = element.getBoundingClientRect();
                if (!rect.width) return;

                const viewX = clientXToViewBoxX(event.clientX, rect);
                const clamped = Math.max(60, Math.min(860, viewX));
                const index = indexForChartX(clamped, current.pointCount);

                if (index !== current.lastIndex) {
                    current.lastIndex = index;
                    current.dotNet.invokeMethodAsync("SetChartHoverIndex", current.chartId ?? "validation-summary", index);
                }
            };

            state.onPointerLeave = () => {
                const current = element.__schoolGrowthValidationHover;
                if (!current?.dotNet || current.lastIndex === -1) return;
                current.lastIndex = -1;
                current.dotNet.invokeMethodAsync("SetChartHoverIndex", current.chartId ?? "validation-summary", -1);
            };

            element.addEventListener("pointermove", state.onPointerMove, { passive: true });
            element.addEventListener("pointerleave", state.onPointerLeave, { passive: true });
            state.attached = true;
        }

        element.__schoolGrowthValidationHover = state;
    }

    function attachById(elementId, dotNet, pointCount, chartId) {
        attach(document.getElementById(elementId), dotNet, pointCount, chartId);
    }

    function clientXToViewBoxX(clientX, rect) {
        const viewWidth = 920;
        const viewHeight = 300;
        const scale = Math.min(rect.width / viewWidth, rect.height / viewHeight);
        if (!Number.isFinite(scale) || scale <= 0) return 0;

        // Default SVG preserveAspectRatio is xMidYMid meet, so wide charts
        // have horizontal letterboxing. Remove that offset before mapping.
        const renderedViewWidth = viewWidth * scale;
        const offsetX = (rect.width - renderedViewWidth) / 2;
        return (clientX - rect.left - offsetX) / scale;
    }

    function indexForChartX(viewX, pointCount) {
        if (pointCount <= 1) return 0;

        const left = 60;
        const width = 800;
        const step = width / (pointCount - 1);
        for (let index = 0; index < pointCount; index++) {
            const start = index === 0
                ? left
                : left + (index - 0.5) * step;
            const end = index === pointCount - 1
                ? left + width
                : left + (index + 0.5) * step;
            if (viewX >= start && viewX <= end) {
                return index;
            }
        }

        return viewX < left ? 0 : pointCount - 1;
    }

    return { attach, attachById };
})();

window.schoolGrowthScroll = (() => {
    function capture() {
        return {
            x: window.scrollX || 0,
            y: window.scrollY || 0
        };
    }

    function restore(position) {
        if (!position) return;
        const x = Number(position.x) || 0;
        const y = Number(position.y) || 0;
        window.requestAnimationFrame(() => {
            window.requestAnimationFrame(() => {
                window.scrollTo(x, y);
            });
        });
    }

    return { capture, restore };
})();
