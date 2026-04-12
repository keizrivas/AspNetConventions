export default {
    mode: "system",
    theme: "dark",
    themeKey: "docmd-theme",
    async init() {

        const theme = localStorage.getItem(this.themeKey);
        if(theme)
        {
            this.mode = theme;
            this.theme = theme;
            this.SetBrowserTheme(this.theme);
            return;
        }

        this.theme = this.isDark() ? "dark" : "light";
        if(this.theme === "dark"){
            return;
        }

        this.SetBrowserTheme(this.theme);

    },
    isDark(){
        return window.matchMedia('(prefers-color-scheme: dark)').matches;
    },
    SetBrowserTheme(theme){
        document.body.setAttribute('data-theme', theme);
        document.documentElement.setAttribute('data-theme', theme);
    },
    SwitchTheme(){
        var theme = this.theme === "dark"
            ? "light" 
            : "dark";

        this.mode = theme;
        this.theme = theme;
        this.SetBrowserTheme(theme);
        localStorage.setItem(this.themeKey, theme)
    }
}