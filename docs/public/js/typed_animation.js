import Typed from "typed.js";

var outputs = [
  {
    endpoint: '\"<span class="text-blue-600 dark:text-blue-400">[controller]</span>/<span class="text-blue-600 dark:text-blue-400">[action]</span>/<span class="text-emerald-600 dark:text-emerald-300">{<span class="text-slate-600 dark:text-slate-400">id</span>}</span>\"',
    result: '`<p class="code-box text-slate-600 dark:text-slate-400 -ml-1!"><span class="text-purple-600 dark:text-purple-300">GET</span> /users/get-user/{id}</p>`'
  },
  {
    endpoint: '\"<span class="text-blue-600 dark:text-blue-400">[controller]</span>/Profile/<span class="text-emerald-600 dark:text-emerald-300">{<span class="text-slate-600 dark:text-slate-400">userId</span>}</span>\"',
    result: '`<p class="code-box text-slate-600 dark:text-slate-400 -ml-1!"><span class="text-purple-600 dark:text-purple-300">GET</span> /users/profile/{user-id}</p>`'
  },
  {
    endpoint: '\"<span class="text-blue-600 dark:text-blue-400">[controller]</span>/Profile/<span class="text-emerald-600 dark:text-emerald-300">{<span class="text-slate-600 dark:text-slate-400">id<span class="text-rose-400 dark:text-rose-300">:</span>int</span>}</span>\"',
    result: '`<p class="code-box text-slate-600 dark:text-slate-400 -ml-1!"><span class="text-purple-600 dark:text-purple-300">GET</span> /users/profile/{id:int}</p>`'
  },
];

var strings = outputs.map(output => {
  return output.endpoint + ' ^1000\n <br>' + output.result;
});

(function() {

  new Typed('#typed', {
    strings,
    typeSpeed: 40,
    backSpeed: 0,
    backDelay: 5000,
    loop: true
  });

})();