(function () {
    var THEME_KEY = 'docmd-theme';

    function getTheme() {
        var stored = localStorage.getItem(THEME_KEY);
        if (stored) return stored;
        var mode = window.DOCMD_DEFAULT_MODE || 'system';
        if (mode === 'system') {
            return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
        }
        return mode;
    }

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        if (document.body) {
            document.body.setAttribute('data-theme', theme);
        }

        var lightLink = document.getElementById('hljs-light');
        var darkLink = document.getElementById('hljs-dark');
        if (lightLink && darkLink) {
            lightLink.disabled = theme === 'dark';
            darkLink.disabled = theme !== 'dark';
        }
    }

    // Tell docmd to use system preference on SPA re-initializations
    window.DOCMD_DEFAULT_MODE = 'system';

    // Apply immediately, overriding docmd's premature 'light' default
    applyTheme(getTheme());
})();
