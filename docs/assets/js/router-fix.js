(function () {
    var DELAY = 80;
    var _pendingHash = null;

    function scrollToHash(hash) {
        var target = document.querySelector(hash);
        if (target) target.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    // Capture the hash from a clicked link before the router strips it.
    document.addEventListener('click', function (e) {
        var anchor = e.target.closest('a[href]');
        var href = anchor && anchor.getAttribute('href');
        var idx = href ? href.indexOf('#') : -1;
        _pendingHash = idx !== -1 ? href.slice(idx) : null;
    }, true);

    // Close the mobile sidebar on every SPA navigation.
    function closeMobileSidebar() {
        document.querySelector('.sidebar')?.classList.remove('mobile-expanded');
    }

    // Restore a hash that the router dropped, then scroll to the target.
    var _push = history.pushState.bind(history);
    history.pushState = function (state, title, url) {
        _push.call(history, state, title, url);
        closeMobileSidebar();
        var hash = _pendingHash;
        _pendingHash = null;
        if (hash) restoreHash(hash);
    };

    function restoreHash(hash) {
        history.replaceState(null, '', window.location.pathname + hash);
        setTimeout(function () { scrollToHash(hash); }, DELAY);
    }

    // Handle back/forward navigation.
    window.addEventListener('popstate', function () {
        var hash = window.location.hash;
        if (hash) setTimeout(function () { scrollToHash(hash); }, DELAY);
    });

    // Handle initial page load with a hash in the URL.
    document.addEventListener('DOMContentLoaded', function () {
        var hash = window.location.hash;
        if (hash) setTimeout(function () { scrollToHash(hash); }, DELAY);
    });
})();
