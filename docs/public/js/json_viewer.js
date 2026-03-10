export default (json) => ({
    raw: json,

    get lines() {
        const parsed = JSON.parse(this.raw)
        const pretty = JSON.stringify(parsed, null, 2)
        return pretty.split('\n').map(line => this.highlight(line))
    },

    highlight(line) {

        return line
            // keys
            .replace(
                /\"(.*?)\":/g,
                '<span class="text-blue-400">"$1"</span>:'
            )
            // strings
            .replace(
                /:\s\"(.*?)\"/g,
                ': <span class="text-emerald-300">"$1"</span>'
            )
            // numbers
            .replace(
                /:\s(-?\d+(?:\.\d+)?)/g,
                ': <span class="text-amber-300">$1</span>'
            )
            // booleans
            .replace(
                /:\s(true|false)/g,
                ': <span class="text-purple-400">$1</span>'
            )
            // null
            .replace(
                /:\s(null)/g,
                ': <span class="text-zinc-400">$1</span>'
            )
    }

});