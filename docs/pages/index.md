---
title: "Home"
description: "Convention-driven standardization for ASP.NET Core"
noStyle: true
components:
  meta: true
  favicon: true
  scripts: false
  css: false
  theme: false
customHead: |
  <link rel="stylesheet" href="./assets/css/main.css">
---

<body x-data="{ 
    isSidebarOpen: false, 
    isScrolled: false,
    copied: false,
    copyToClipboard(text) {
      navigator.clipboard.writeText(text);
      this.copied = true;
      setTimeout(() => this.copied = false, 2000);
    },
    init() {
      this.isScrolled = window.scrollY > 20;
    }
  }" 
  @scroll.window="isScrolled = window.scrollY > 20">

  <!-- Navbar -->
  <nav :class="isScrolled ? 'glass-nav py-3' : 'bg-transparent py-6'" class="fixed top-0 left-0 right-0 z-50 w-full border border-transparent transition-all duration-300">
    <div class="max-w-7xl mx-auto px-6 flex items-center justify-between">
      <div class="flex flex-1 items-center">
        <a href="/AspNetConventions" class="max-w-60 flex items-center gap-2">
          <img src="assets/images/logo_light.svg" alt="AspNetConventions" class="dark:hidden block">
          <img src="assets/images/logo_dark.svg" alt="AspNetConventions" class="dark:block hidden">
        </a>
      </div>
      <div 
        :class="{
          'max-md:mt-0!': isScrolled,
          'navbar-dropdown' : isSidebarOpen
        }" 
        class="transition-all duration-300 hidden w-full mx-auto md:block md:w-auto z-50">
        <ul class="font-medium flex items-center justify-center flex-col space-y-2 md:space-y-0 p-4 md:p-0 rounded-lg md:flex-row md:space-x-2">
          <li class="w-full">
            <a href="#features" class="navbar-dropdown-item" aria-current="page">
              Features
            </a>
          </li>
          <li class="w-full">
            <a href="/AspNetConventions/docs" class="navbar-dropdown-item">
              Documentation
            </a>
          </li>
          <li class="hidden md:block">
            <span class="opacity-25 px-1">|</span>
          </li>
          <li class="w-full">
            <a href="https://github.com/keizrivas/AspNetConventions" target="_blank" rel="noopener noreferrer" class="navbar-dropdown-item flex! justify-between items-center">
              <div class="flex">
                <svg class="w-5 h-5" role="img" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                  <title>GitHub</title>
                  <path fill="currentColor" d="M12 .297c-6.63 0-12 5.373-12 12 0 5.303 3.438 9.8 8.205 11.385.6.113.82-.258.82-.577 0-.285-.01-1.04-.015-2.04-3.338.724-4.042-1.61-4.042-1.61C4.422 18.07 3.633 17.7 3.633 17.7c-1.087-.744.084-.729.084-.729 1.205.084 1.838 1.236 1.838 1.236 1.07 1.835 2.809 1.305 3.495.998.108-.776.417-1.305.76-1.605-2.665-.3-5.466-1.332-5.466-5.93 0-1.31.465-2.38 1.235-3.22-.135-.303-.54-1.523.105-3.176 0 0 1.005-.322 3.3 1.23.96-.267 1.98-.399 3-.405 1.02.006 2.04.138 3 .405 2.28-1.552 3.285-1.23 3.285-1.23.645 1.653.24 2.873.12 3.176.765.84 1.23 1.91 1.23 3.22 0 4.61-2.805 5.625-5.475 5.92.42.36.81 1.096.81 2.22 0 1.606-.015 2.896-.015 3.286 0 .315.21.69.825.57C20.565 22.092 24 17.592 24 12.297c0-6.627-5.373-12-12-12"/>
                </svg>
                <span class="block md:hidden ml-2.5">Github</span>
              </div>
              <i data-lucide="square-arrow-out-up-right" class="block md:hidden w-5 h-5"></i>
            </a>
          </li>
          <li class="w-full">
            <button @click="$store.Appearance.SwitchTheme()" class="navbar-dropdown-item w-full cursor-pointer flex! items-center">
              <i x-show="$store.Appearance.theme === 'light'" data-lucide="sun" class="w-5 h-5"></i>
              <i x-show="$store.Appearance.theme === 'dark'" data-lucide="moon" class="w-5 h-5"></i>
              <span class="capitalize block md:hidden ml-2.5">
                <span x-text="$store.Appearance.theme"></span> Mode
              </span>
            </button>
          </li>
        </ul>
      </div>
      <button class="navbar-dropdown-item md:hidden! cursor-pointer" @click="isSidebarOpen = !isSidebarOpen">
        <i x-show="!isSidebarOpen" data-lucide="menu" class="w-5 h-5"></i>
        <i x-show="isSidebarOpen" data-lucide="x" class="w-5 h-5"></i>
      </button>
    </div>
  </nav>

  <!-- Hero Section -->
  <section class="relative pt-24 pb-24 px-6 w-full2 overflow-hidden z-10">
    <div class="absolute top-0 left-1/2 -translate-y-1/2 -translate-x-1/2  w-full max-w-4xl h-40 bg-brand-purple/25 dark:bg-brand-purple/10 blur-[120px] rounded-full -z-10"></div>
    <div class="max-w-7xl mx-auto text-center">
      <a class="package-box mb-8"
        href="https://www.nuget.org/packages/AspNetConventions" target="_blank" rel="noopener noreferrer">
          <svg role="img" class="w-3.5 h-3.5" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
            <title>NuGet</title>
            <path fill="currentColor" d="M1.998.342a1.997 1.997 0 1 0 0 3.995 1.997 1.997 0 0 0 0-3.995zm9.18 4.34a6.156 6.156 0 0 0-6.153 6.155v6.667c0 3.4 2.756 6.154 6.154 6.154h6.667c3.4 0 6.154-2.755 6.154-6.154v-6.667a6.154 6.154 0 0 0-6.154-6.155zm-1.477 2.8a2.496 2.496 0 1 1 0 4.993 2.496 2.496 0 0 1 0-4.993zm7.968 6.16a3.996 3.996 0 1 1-.002 7.992 3.996 3.996 0 0 1 .002-7.992z"/>
          </svg>
          <span class="font-mono" x-text="$store.packageMetadata.version"></span> Now available
      </a>
      <h1 class="text-5xl md:text-7xl font-extrabold tracking-tight leading-tight mb-6  ">
        Dynamic
        <span class="bg-linear-to-r from-brand-pink via-purple-500 to-brand-purple text-transparent bg-clip-text animate-gradient">
          Conventions
        </span>
        <br>
        For ASP.NET
      </h1>
      <p class="text-2xl text-slate-500 mb-10 max-w-3xl mx-auto leading-relaxed">
        A powerful and zero boilerplate solution for consistent standardization in modern ASP.NET apps.
      </p>
      <div class="flex flex-wrap justify-center items-center gap-4">
        <a href="/docs" class="group relative inline-flex items-center justify-center overflow-hidden rounded-full duration-500 font-medium text-white bg-slate-950 dark:text-slate-950 dark:bg-white h-12 px-6">
          <div class="relative inline-flex translate-x-0 items-center transition group-hover:-translate-x-6">
            <div class="absolute translate-x-0 opacity-100 transition group-hover:-translate-x-6 group-hover:opacity-0">
              <i data-lucide="book_open_text" class="w-4.5 h-4.5"></i>
            </div>
            <span class="pl-6">Get Started</span>
            <div class="absolute right-0 translate-x-12 opacity-0 transition group-hover:translate-x-6 group-hover:opacity-100">
              <i data-lucide="arrow-right" class="w-4.5 h-4.5"></i>
            </div>
          </div>
        </a>
        <div class="card p-1 relative rounded-full before:rounded-full! after:rounded-full!">
          <div class="glow"></div>
          <div class="px-8 py-4 font-mono text-sm flex items-center gap-3 rounded-full cursor-pointer border border-white/10 -m-1"
            @click="copyToClipboard('dotnet add package AspNetConventions')">
            <i data-lucide="terminal" class="text-brand-purple w-4.5 h-4.5"></i>
            dotnet add package AspNetConventions
            <div x-show="copied">
              <i data-lucide="check" class="text-green-500 w-4 h-4"></i>
            </div>
            <div x-show="!copied">
              <i data-lucide="copy" class="text-slate-500 w-4 h-4"></i>
            </div>
          </div>
        </div>
      </div>
      <div class="mt-16 flex items-center justify-center gap-2 sm:gap-8">
        <div class="flex items-center gap-2">
          <i data-lucide="star" class="w-4.5 h-4.5"></i>
          <span class="font-medium font-mono" x-text="Formatter.format($store.packageMetadata.stargazersCount)"></span> 
          <span class="text-slate-500">Stars</span>
        </div>
        <div class="flex items-center gap-2">
          <i data-lucide="download" class="w-4.5 h-4.5"></i>
          <span class="font-medium font-mono" x-text="Formatter.format($store.packageMetadata.totalDownloads)"></span> 
          <span class="text-slate-500">Downloads</span>
        </div>
        <div class="flex items-center gap-2">
          <i data-lucide="git-fork" class="w-4.5 h-4.5"></i>
          <span class="font-medium font-mono" x-text="Formatter.format($store.packageMetadata.forksCount)"></span> 
          <span class="text-slate-500">Forks</span>
        </div>
      </div>
    </div>
    <div class="absolute -rotate-45 -right-32 -bottom-20 md:-bottom-32 -z-10 w-100 h-100 md:w-135 md:h-135 lg:w-150 lg:h-150 opacity-85 dark:opacity-70">
      <img src="assets/images/logo_3d-alpha.png" alt="3d">
    </div>
  </section>

  <!-- features Section -->
  <section id="features" class="py-24 w-full bg-slate-100 dark:bg-dark-200">
    <div class="mx-auto max-w-2xl px-0.5 min-[360px] md:px-6 lg:max-w-7xl lg:px-8">
      <h2 class="text-center text-base/7 font-semibold text-brand-purple">
        Features
      </h2>
      <p class="mx-auto mt-2 max-w-7xl text-center sm:text-4xl font-semibold tracking-tight text-balance">
        Beautifully Structured Asp.Net applications through Consistent Conventions
      </p>
      <div class="mt-10 grid gap-4 sm:mt-16 lg:grid-cols-3 lg:grid-rows-2">
        <div class="card relative lg:row-span-2 before:lg:rounded-l-4xl! after:lg:rounded-l-4xl!">
          <div class="glow"></div>
          <div class="relative flex h-full flex-col lg:rounded-l-4xl overflow-hidden">
            <a href="/AspNetConventions/docs/route-standardization/" class="group px-4 pt-8 pb-3 sm:px-10 sm:pt-10 sm:pb-0">
              <p class="group-hover:underline flex items-center mt-2 text-lg font-medium tracking-tight max-lg:text-center">
                <i data-lucide="link" class="w-4.5 h-4.5 mr-2"></i> Route Standarization
              </p>
              <p class="mt-2 max-w-lg text-sm/6 text-slate-500 max-lg:text-center">
                Turn complex, inconsistent URLs into clean, SEO-friendly paths automatically.
              </p>
            </a>
            <div class="flex flex-col gap-4 px-2 pt-2 sm:px-6 pb-8 mt-4 h-full">
              <div class="relative z-10">
                <div class="flex flex-col gap-y-3 bg-glass shadow-xl rounded-3xl p-3 mt-4">
                  <div class="flex gap-x-1.5 ml-1.5 mt-1 mb-2">
                    <div class="rounded-full bg-dark-50/20 dark:bg-dark-50 w-2 h-2"></div>
                    <div class="rounded-full bg-dark-50/20 dark:bg-dark-50 w-2 h-2"></div>
                    <div class="rounded-full bg-dark-50/20 dark:bg-dark-50 w-2 h-2"></div>
                  </div>
                  <div class="flex items-center font-medium text-xs max-w-52 text-slate-600 dark:text-slate-400 border border-black/5 dark:border-white/5 bg-dark-50/10 dark:bg-dark-50/70 backdrop-blur-sm px-4 py-2.5 rounded-xl">
                    <span class="font-bold text-emerald-600 dark:text-emerald-400 pr-2">C#</span>
                    UsersController.cs
                    <i data-lucide="x" class="w-5 h-5 ml-2.5"></i>
                  </div>
                  <div class="bg-slate-100 dark:bg-dark-300 border border-black/10 dark:border-white/5 rounded-xl font-mono text-sm whitespace-nowrap min-h-52 p-4">
                    <p class="text-slate-500 pb-2 mt-2.5">// Get user data </p>
                    <span id="typed"></span>
                  </div>
                  <div class="flex ml-1.5 gap-x-2">
                    <div class="bg-blue-400/40 rounded-2xl w-5 p-1.5"></div>
                    <div class="bg-dark-50/20 dark:bg-dark-50/80 rounded-2xl w-24 p-1.5"></div>
                  </div>
                </div>
              </div>
              <div class="absolute flex justify-center left-1/2 -translate-x-1/2 -bottom-24">
                <div class="gradient-ring">
                  <div class="inner-content-ring flex justify-center items-center">
                    <div class="wave-ring"></div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="card max-lg:row-start-1 before:max-lg:rounded-t-4xl! after:max-lg:rounded-t-4xl!">
          <div class="glow"></div>
          <div class="relative flex h-full flex-col rounded-2xl max-lg:rounded-t-4xl min-h-72 overflow-hidden">
            <a href="/AspNetConventions/docs/exception-handling/" class="group px-4 pt-8 sm:px-10 sm:pt-10">
              <p class="group-hover:underline flex items-center mt-2 text-lg font-medium tracking-tight max-lg:text-center">
                <i data-lucide="link" class="w-4.5 h-4.5 mr-2"></i> Exception Handling
              </p>
              <p class="mt-2 max-w-lg text-sm/6 text-slate-500 max-lg:text-center">
                A safety net that intercepts, map and formats every failure.
              </p>
            </a>
            <div class="flex-1 px-4 sm:px-10 py-5 relative">
              <div class="relative flex flex-col gap-2.5 z-10 pr-24 lg:pr-2 xl:pr-18">
                <div class="flex gap-2 font-mono text-sm bg-glass shadow-lg rounded-xl px-4 py-2">
                  <i data-lucide="info" class="text-pink-500 w-5 min-w-5 h-5"></i> ValidationException
                </div>
                <div class="flex gap-2 font-mono text-sm bg-glass shadow-lg rounded-xl px-4 py-2 ml-4 -mr-4">
                  <i data-lucide="bug" class="text-pink-500 w-5 min-w-5 h-5"></i> ArgumentNullException
                </div>
                <div class="flex gap-2 font-mono text-sm bg-glass shadow-lg rounded-xl px-4 py-2">
                  <i data-lucide="shield-alert" class="text-pink-500 w-5 min-w-5 h-5"></i> SecurityException
                </div>
              </div>
              <img src="./assets/images/error.svg" alt="" class="absolute h-42 top-4 right-4 opacity-75" />
            </div>
          </div>
        </div>
        <div class="card card-gradient relative max-lg:row-start-3 lg:col-start-2 lg:row-start-2 text-white">
          <div class="glow"></div>
          <div class="bg-noise absolute inset-px rounded-2xl opacity-75 bg-black/25 dark:bg-black/50"></div>
          <div class="relative flex flex-col h-full min-h-72 overflow-hidden">
            <a href="/AspNetConventions/docs/json-serialization/" class="group px-4 pt-8 sm:px-10 sm:pt-10">
              <p class="group-hover:underline flex items-center mt-2 text-lg font-medium tracking-tight max-lg:text-center">
                <i data-lucide="link" class="w-4.5 h-4.5 mr-2"></i> JSON Serialization
              </p>
              <p class="mt-2 max-w-lg text-sm/6 text-slate-200 max-lg:text-center">
                Global Json serialization control with zero external dependencies.
              </p>
            </a>
            <div class="flex-1 py-5 px-4 sm:px-10 relative">
              <div class="relative flex flex-col gap-2.5 z-10 pr-24 lg:pr-2 xl:pr-18">
                <div class="ml-3.5">
                  <ol class="list-disc font-mono space-y-1.5 text-sm">
                    <li class="opacity-100"> 
                      WriteIndented
                    </li>
                    <li class="opacity-80"> 
                      AllowTrailingCommas
                    </li>
                    <li class="opacity-60"> 
                      NumberHandling
                    </li>
                    <li class="opacity-40"> 
                      Converters
                    </li>
                    <li class="opacity-20"> 
                      SerializerOptions
                    </li>
                    <li class="opacity-5"> 
                      MaxDepth
                    </li>
                  </ol>
                </div>
              </div>
              <img src="./assets/images/json.svg" alt="" class="absolute mix-blend-overlay h-56 top-3 right-3 opacity-75" />
            </div>
          </div>
        </div>
        <div class="card card-editor relative lg:row-span-2 p-0.5 before:max-lg:rounded-b-4xl! before:lg:rounded-r-4xl! after:max-lg:rounded-b-4xl! after:lg:rounded-r-4xl!">
          <div class="glow"></div>
          <div class="h-full w-full rounded-2xl max-lg:rounded-b-4xl lg:rounded-r-4xl overflow-hidden">
            <div class="relative flex h-full flex-col">
              <a href="/AspNetConventions/docs/response-formatting/" class="group px-4 pt-8 pb-3 sm:px-10 sm:pt-10 sm:pb-0">
                <p class="group-hover:underline flex items-center mt-2 text-lg font-medium tracking-tight max-lg:text-center">
                  <i data-lucide="link" class="w-4.5 h-4.5 mr-2"></i> Response Formatting
                </p>
                <p class="mt-2 max-w-lg text-sm/6 text-slate-500 max-lg:text-center">
                  Elegant and customizable response models for every endpoint across your API.
                </p>
              </a>
              <div class="relative min-h-120 w-full grow">
                <div class="absolute top-10 right-0 bottom-0 left-6 rounded-tl-3xl outline outline-black/20 dark:outline-white/10 border-glass -mb-0.5 -mr-0.5">
                  <div class="absolute inset-px mt-2 ml-2 rounded-tl-2xl text-black dark:text-white bg-slate-100 dark:bg-dark-300 outline outline-black/10 dark:outline-white/5 overflow-hidden">
                    <div class="flex bg-slate-200 dark:bg-dark-300 outline outline-black/10 dark:outline-white/5 -mb-1">
                      <div class="-mb-px flex text-sm/6 font-medium w-full">
                        <div class="w-full border-r border-b border-white/10 bg-white/5 px-4 py-2 dark:text-white whitespace-nowrap flex items-center">
                          <div class="flex items-center pr-2">
                            <span class="font-bold text-emerald-600 dark:text-emerald-400 pr-2">POST</span>
                            <i data-lucide="chevron-down" class="w-5 h-5"></i>
                          </div> 
                          <div class="flex-1 whitespace-nowrap my-1 mx-1 bg-white/5 rounded-xl px-2 py-2 font-mono dark:text-slate-200">
                            <span class="rounded-lg text-white bg-brand-purple px-1.5 py-0.5">_.base_url</span>
                            /api/transactions
                          </div>
                          <button class="font-bold ml-2 my-1 flex items-center justify-center gap-2 rounded-xl px-2.5 py-1.5 text-white bg-brand-purple">
                            Send 
                            <i data-lucide="chevron-down" class="w-5 h-5"></i>
                          </button>
                        </div>
                      </div>
                    </div>
                    <div class="text-sm pt-4 pb-24 overflow-hidden" x-data="jsonViewer($store.jsonResponse)">
                        <template x-for="(line, index) in lines" :key="index">
                          <div class="flex">
                            <div class="w-12 min-w-12 text-center p-0.5 select-none text-slate-500"
                              x-text="index + 1">
                            </div>
                            <pre><p x-html="line" class="overflow-hidden"></p></pre>
                          </div>
                      </template>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>

  <!-- Footer -->
  <footer>
      <div class="mx-auto w-full max-w-7xl p-4 py-6 lg:py-8">
          <div class="md:flex md:justify-between md:items-center">
            <div class="mb-6 md:mb-0">
                <a href="/AspNetConventions/" class="flex items-center">
                    <img src="./assets/images/logo.svg" class="w-24 me-3" alt="AspNetConventions" />
                </a>
            </div>
            <div class="grid grid-cols-1 gap-8 sm:gap-6 sm:grid-cols-2 text-sm">
                <div>
                    <h2 class="mb-6 text-sm font-semibold">Source</h2>
                    <ul class="text-slate-500">
                        <li class="mb-4">
                            <a href="https://www.nuget.org/packages/AspNetConventions" class="hover:underline">NuGet</a>
                        </li>
                        <li class="mb-4">
                            <a href="https://github.com/keizrivas/AspNetConventions" class="hover:underline">GitHub</a>
                        </li>
                        <li>
                            <a href="https://github.com/keizrivas/AspNetConventions/releases" class="hover:underline">Releases</a>
                        </li>
                    </ul>
                </div>
                <div>
                    <h2 class="mb-6 text-sm font-semibold">Features</h2>
                    <ul class="text-slate-500">
                        <li class="mb-4">
                            <a href="/AspNetConventions/docs/route-standardization/" class="hover:underline">Route Standarization</a>
                        </li>
                        <li class="mb-4">
                            <a href="/AspNetConventions/docs/response-formatting/" class="hover:underline">Response Formatting</a>
                        </li>
                        <li class="mb-4">
                            <a href="/AspNetConventions/docs/exception-handling/" class="hover:underline">Exception Handling</a>
                        </li>
                        <li>
                            <a href="/AspNetConventions/docs/json-serialization/" class="hover:underline">JSON Serialization</a>
                        </li>
                    </ul>
                </div>
            </div>
        </div>
        <hr class="my-6 border-slate-400 dark:border-slate-600 sm:mx-auto lg:my-8 opacity-50" />
        <div class="sm:flex sm:items-center sm:justify-between">
            <p class="text-sm sm:text-center text-slate-400 dark:text-slate-600" x-data="{ year: new Date().getFullYear() }">
              Open Source under MIT License | © <span x-text="year"></span> AspNetConventions
            </p>
        </div>
      </div>
  </footer>
  <script src="./assets/js/main.js"></script>
</body>