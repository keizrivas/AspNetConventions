import Alpine from 'alpinejs'
import PackageMetadata from './package_metadata.js'
import Appearance from './appearance.js'
import JsonViewer from './json_viewer.js'
import './typed_animation.js'
import './glowing_animation.js'
import { 
    createIcons, 
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
    X,
    ChevronDown,
    ShieldAlert,
    Bug,
    Info,
    SquareArrowOutUpRight,
    Link
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

    // Page dark/light appearance
    Alpine.store('Appearance', Appearance);

    // Register the jsonViewer component
    Alpine.data('jsonViewer', JsonViewer);

    // Register the jsonResponse component
    var json = `{"status":"success","statusCode":201,"message":"Transaction created successfully.","data":{"transactionId":"BNK_566C_UT567990-8","amount":250.38,"currency":"USD","fromAccount":"****4582","toAccount":"****9174","status":"pending","createdAt":"[DATE]"},"metadata":{"requestType":"POST","timestamp":"[DATE]","traceId":"00-ed89d1cc507c35126d6f0e933984f774-99b8b9a3feb75652-00","path":"/api/transactions"}}`;
    var currentDate = new Date().toISOString();
    json = json.replace(/\[DATE\]/g, currentDate);
    Alpine.store('jsonResponse', json);

    // Start Alpine.js after DOM is ready
    Alpine.start();

    // Built Lucide icons 
    createIcons({
        icons: {
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
            X,
            ChevronDown,
            ShieldAlert,
            Bug,
            Info,
            SquareArrowOutUpRight,
            Link
        }
    });

})();

