(function () {
    var DELAY = 80;
    var _pendingHash = null;

    function scrollToHash(hash) {
        var target = document.querySelector(hash);
        if (target) target.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    // Strip /index from a URL string so the SPA framework never sees it.
    // Stripping before pushState (not after via replaceState) ensures the
    // framework resolves CSS and asset paths against the clean URL.
    function stripIndex(url) {
        return typeof url === 'string' && url.endsWith('/index')
            ? url.slice(0, -6)
            : url;
    }

    // Clean /index from the current address bar entry — used for initial
    // page load and popstate, where we can't intercept the URL beforehand.
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

    // Patch pushState: strip /index from the URL *before* committing it so
    // the SPA framework always receives and renders with the clean URL.
    // Previously we called _push first then replaceState, which let the
    // framework briefly see the /index URL and resolve assets against it.
    var _push = history.pushState.bind(history);
    history.pushState = function (state, title, url) {
        _push.call(history, state, title, stripIndex(url));
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
