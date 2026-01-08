window.dorkScrollToBottom = (el) => {
    if (!el) return;
    el.scrollTop = el.scrollHeight;
};
