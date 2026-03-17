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
    optionsMenu: {
      position: 'header', //sidebar-top
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
    appearance: 'system',
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
    { 
      title: 'Overview', 
      path: 'docs/', 
      icon: 'home' 
    },
    {
      title: 'Getting Started',
      icon: 'rocket',
      collapsible: true,
      children: [
        { 
          title: 'Installation', 
          path: 'docs/', 
          icon: 'terminal', 
        },
        { 
          title: 'Configuration', 
          path: 'docs/', 
          icon: 'settings', 
        },
        { 
          title: 'Basic Usage', 
          path: 'docs/', 
          icon: 'braces', 
        },
      ],
    },
    {
      title: 'Route Standarization',
      icon: 'rocket',
      collapsible: true,
      children: [
      ],
    },
    {
      title: 'Response Formatting',
      icon: 'rocket',
      collapsible: true,
      children: [
      ],
    },
    {
      title: 'Exception Handling',
      icon: 'rocket',
      collapsible: true,
      children: [
      ],
    },
    {
      title: 'JSON Serialization',
      icon: 'rocket',
      collapsible: true,
      children: [
      ],
    },
    { 
      title: 'NuGet', 
      path: 'https://www.nuget.org/packages/AspNetConventions', 
      icon: 'box', 
      external: true 
    },
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
