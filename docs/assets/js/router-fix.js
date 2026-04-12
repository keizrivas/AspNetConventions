(function () {
    var DELAY = 80;
    var _pendingHash = null;

    function scrollToHash(hash) {
        var target = document.querySelector(hash);
        if (target) target.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    // docmd generates /index suffixes when linking to index.md files — strip them.
    function cleanIndexUrl() {
        if (!window.location.pathname.endsWith('/index')) return;
        var clean = window.location.pathname.slice(0, -6);
        history.replaceState(null, '', clean + window.location.search + window.location.hash);
    }

    // Restore a hash that the router dropped, then scroll to the target.
    function restoreHash(hash) {
        history.replaceState(null, '', window.location.pathname + hash);
        setTimeout(function () { scrollToHash(hash); }, DELAY);
    }

    // Capture the hash from a clicked link before the router strips it.
    document.addEventListener('click', function (e) {
        var anchor = e.target.closest('a[href]');
        var href = anchor && anchor.getAttribute('href');
        var idx = href ? href.indexOf('#') : -1;
        _pendingHash = idx !== -1 ? href.slice(idx) : null;
    }, true);

    // Patch pushState: clean the URL, then restore any pending hash.
    var _push = history.pushState.bind(history);
    history.pushState = function () {
        _push.apply(history, arguments);
        cleanIndexUrl();
        var hash = _pendingHash;
        _pendingHash = null;
        if (hash) restoreHash(hash);
    };

    // Handle back/forward navigation.
    window.addEventListener('popstate', function () {
        cleanIndexUrl();
        var hash = window.location.hash;
        if (hash) setTimeout(function () { scrollToHash(hash); }, DELAY);
    });

    // Handle initial page load with a hash in the URL.
    document.addEventListener('DOMContentLoaded', function () {
        cleanIndexUrl();
        var hash = window.location.hash;
        if (hash) setTimeout(function () { scrollToHash(hash); }, DELAY);
    });
})();
