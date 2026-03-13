// docmd.config.js
module.exports = {
  // --- Core Metadata ---
  siteTitle: 'AspNetConventions',
  siteUrl: '', 

  // --- Branding ---
  logo: {
    light: 'assets/images/logo_light.svg',
    dark: 'assets/images/logo_dark.svg',
    alt: 'AspNetConventions',
    href: '/',
  },
  favicon: 'assets/favicon.ico',

  // --- Source & Output ---
  srcDir: 'pages',
  outputDir: 'dist',

  // --- Layout & UI Architecture (V2) ---
  layout: {
    spa: true,
    header: {
      enabled: true,
    },
    sidebar: {
      collapsible: true,
      defaultCollapsed: false,
    },
    // Centralized Options Menu (Search, Theme, Sponsor)
    optionsMenu: {
      position: 'sidebar-top',
      components: {
        search: true,
        themeSwitch: true,
        sponsor: null,
      }
    },
    // Footer Configuration
    footer: {
      style: 'minimal',
      content: 'Open Source under MIT License | ©' + new Date().getFullYear() + ' AspNetConventions.',
    }
  },

  // --- Theme Settings ---
  theme: {
    name: 'default',
    defaultMode: 'system',
    codeHighlight: true, 
    customCss: [ 
      '/assets/css/asp_net_conventions.theme.css',
    ],
  },

  // --- General Features ---
  minify: true,
  autoTitleFromH1: true,
  copyCode: true,
  pageNavigation: true,
  
  customJs: [
    // '/assets/js/main.js',
  ],

  // --- Navigation (Sidebar) ---
  navigation: [
    { title: 'Introduction', path: 'docs/getting-started', icon: 'home' },
    // {
    //   title: 'Guide',
    //   icon: 'book-open',
    //   collapsible: true,
    //   children: [
    //     { title: 'Getting Started', path: 'https://docs.docmd.io/getting-started/installation', icon: 'rocket', external: true },
    //     { title: 'Configuration', path: 'https://docs.docmd.io/configuration', icon: 'settings', external: true },
    //   ],
    // },
    { 
      title: 'GitHub', 
      path: 'https://github.com/keizrivas/AspNetConventions', 
      icon: 'github', 
      external: true 
    },
  ],

  // --- Plugins ---
  plugins: {
    seo: {
      defaultDescription: 'Convention-driven standardization for ASP.NET Core',
      openGraph: {
        defaultImage: '../assets/asp_net_conventions.png',
      },
      twitter: {
        cardType: 'summary_large_image',
      }
    },
    sitemap: {
      defaultChangefreq: 'weekly',
      defaultPriority: 0.8
    },
    // analytics: {
    //   googleV4: { measurementId: '' }
    // },
    search: {},
    mermaid: {},
    llms: {}
  },
  
  // --- Edit Link ---
  editLink: {
    enabled: false,
    baseUrl: 'https://github.com/keizrivas/AspNetConventions/edit/main/docs',
    text: 'Edit this page'
  }
};
