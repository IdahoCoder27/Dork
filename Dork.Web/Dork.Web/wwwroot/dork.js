window.dorkScrollToBottom = (el) => {
    if (!el) return;
    requestAnimationFrame(() => {
        el.scrollTop = el.scrollHeight;
    });
};

