export default {
    name: "AspNetConventions",
    fullName: "AspNetConventions",
    version: "1.0.0",
    authors: [],
    forksCount: 0,
    watchersCount: 0,
    totalDownloads: 0,
    stargazersCount: 0, 
    async init() {

        const CACHE_KEY = "aspnet_conventions_data";
        const CACHE_DURATION = 30 * 60 * 1000; // 30 minutes
        const now = new Date().getTime();

        // Check if valid cache exists
        const cached = localStorage.getItem(CACHE_KEY);
        if (cached) {
            const { data, expiry } = JSON.parse(cached);
            if (now < expiry) {
                console.log("Using cached package metadata");
                this.applyData(data.github, data.nuget);
                return;
            }
        }

        const urls = [
            "https://api.github.com/repos/keizrivas/AspNetConventions",
            "https://azuresearch-usnc.nuget.org/query?q=packageid:AspNetConventions"
        ];

        try {
            const [github, nuget] = await Promise.all(
                urls.map(url => fetch(url).then(res => res.json()))
            );

            // Save to localStorage
            localStorage.setItem(CACHE_KEY, JSON.stringify({
                expiry: now + CACHE_DURATION,
                data: { github, nuget }
            }));

            // Map data to store
            this.applyData(github, nuget);

        } catch (error) {
            console.error('Fetch failed:', error);
        }
    },
    applyData(github, nuget) {
        // Set Github info
        this.name = github.name;
        this.fullName = github.full_name;
        this.stargazersCount = github.stargazers_count || this.stargazersCount;
        this.watchersCount = github.watchers_count || this.watchersCount;
        this.forksCount = github.forks_count || this.forksCount;

        // Set NuGet info
        if (nuget.data && nuget.data.length > 0) {
            const packageInfo = nuget.data[0];
            this.version = packageInfo.version || this.version;
            this.authors = packageInfo.authors || this.authors;
            this.totalDownloads = packageInfo.totalDownloads || this.totalDownloads;
        }
    }
}