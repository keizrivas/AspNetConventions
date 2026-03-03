import Alpine from 'alpinejs'
import PackageMetadata from './package_metadata.js'
import { 
    createIcons, 
    Github, 
    Terminal,
    Menu,
    Check,
    Copy,
    Star,
    GitFork,
    Download,
    Package,
    ArrowRight,
    BookOpenText,
    Sun,
    Moon,
} from 'lucide';

(function() {

    // Setup number formatter for compact display
    window.Formatter = new Intl.NumberFormat("en-US", {
        notation: "compact",
        compactDisplay: "short"
    });

    // Setup Alpine.js
    window.Alpine = Alpine

    // Register the packageMetadata component
    Alpine.store('packageMetadata', PackageMetadata);

    // Start Alpine.js after DOM is ready
    Alpine.start();

    // Built Lucide icons 
    createIcons({
        icons: {
            Github,
            Terminal,
            Menu,
            Check,
            Copy,
            Star,
            GitFork,
            Download,
            Package,
            ArrowRight,
            BookOpenText,
            Sun,
            Moon,
        }
    });

})();

