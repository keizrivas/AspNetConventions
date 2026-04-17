export default {
  title: "AspNetConventions",
  url: "https://keizrivas.github.io/AspNetConventions",
  base: "/AspNetConventions/",

  logo: {
    light: "assets/images/logo_light.svg",
    dark: "assets/images/logo_dark.svg",
    alt: "AspNetConventions",
    href: "https://keizrivas.github.io/AspNetConventions/"
  },
  favicon: "assets/favicon.ico",
  src: "pages",
  out: "site",
  layout: {
    spa: true,
    header: {
      enabled: true
    },
    sidebar: {
      collapsible: true,
      defaultCollapsed: false
    },
    optionsMenu: {
      position: "header",
      components: {
        search: true,
        themeSwitch: true,
        sponsor: null
      }
    },
    footer: {
      style: "minimal",
      content: "Open Source under MIT License | ©2026 AspNetConventions."
    }
  },
  theme: {
    name: "default",
    appearance: "system",
    codeHighlight: true,
    customCss: [
      "/assets/css/asp_net_conventions.theme.css"
    ]
  },
  minify: true,
  autoTitleFromH1: true,
  copyCode: true,
  pageNavigation: true,
  customJs: [],
  editLink: {
    enabled: false,
    baseUrl: "https://github.com/keizrivas/AspNetConventions/edit/main/docs",
    text: "Edit this page"
  },
  plugins: {
    seo: {
      defaultDescription: "Convention-driven standardization for ASP.NET Core",
      openGraph: {
        defaultImage: "assets/images/asp_net_conventions.png"
      },
      twitter: {
        cardType: "summary_large_image"
      }
    },
    sitemap: {
      defaultChangefreq: "weekly",
      defaultPriority: 0.8
    },
    search: {},
    mermaid: {},
    llms: {}
  },
  customJs: [
    '/assets/js/theme.js',
    '/assets/js/router-fix.js',
  ],
  navigation: [
    {
      title: "Introduction",
      path: "docs/",
      icon: "home"
    },
    {
      title: "Getting Started",
      icon: "rocket",
      collapsible: false,
      children: [
        {
          title: "Overview",
          path: "docs/getting-started/"
        },
        {
          title: "Quick Start",
          path: "docs/getting-started/quick-start"
        },
        {
          title: "Basic Usage",
          path: "docs/getting-started/basic-usage"
        }
      ]
    },
    {
      title: "Configuration Reference",
      path: "docs/configuration-reference",
      icon: "settings"
    },
    {
      title: "Route Standardization",
      icon: "link",
      collapsible: true,
      children: [
        {
          title: "Overview",
          path: "docs/route-standardization"
        },
        {
          title: "Configuration",
          path: "docs/route-standardization/configuration"
        },
        {
          title: "Parameter Binding",
          path: "docs/route-standardization/parameter-binding"
        },
      ]
    },
    {
      title: "Response Formatting",
      icon: "file-braces-corner",
      collapsible: true,
      children: [
        {
          title: "Overview",
          path: "docs/response-formatting"
        },
        {
          title: "Configuration",
          path: "docs/response-formatting/configuration"
        },
        {
          title: "ApiResults",
          path: "docs/response-formatting/api-results"
        },
        {
          title: "Custom Response Builders",
          path: "docs/response-formatting/custom-response-builders"
        },
        {
          title: "Metadata",
          path: "docs/response-formatting/metadata"
        },
      ]
    },
    {
      title: "Exception Handling",
      icon: "shield-alert",
      collapsible: true,
      children: [
        {
          title: "Overview",
          path: "docs/exception-handling"
        },
        {
          title: "Configuration",
          path: "docs/exception-handling/configuration"
        },
        {
          title: "Exception Mappers",
          path: "docs/exception-handling/exception-mappers"
        },
      ]
    },
    {
      title: "JSON Serialization",
      icon: "braces",
      collapsible: true,
      children: [
        {
          title: "Overview",
          path: "docs/json-serialization"
        },
        {
          title: "Configuration",
          path: "docs/json-serialization/configuration"
        },
        {
          title: "Features",
          path: "docs/json-serialization/features"
        },
      ]
    },
    {
      title: "Troubleshooting",
      path: "docs/troubleshooting",
      icon: "circle-help"
    },
    {
      title: "Extension methods",
      path: "docs/extension-methods",
      icon: "puzzle"
    },
    {
      title: "NuGet",
      path: "https://www.nuget.org/packages/AspNetConventions",
      external: true
    },
    {
      title: "GitHub",
      path: "https://github.com/keizrivas/AspNetConventions",
      external: true
    }
  ]
};
