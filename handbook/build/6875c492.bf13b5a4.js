(window.webpackJsonp=window.webpackJsonp||[]).push([[40,11],{154:function(e,a,t){"use strict";var n=t(151);t.d(a,"a",(function(){return n.a}))},95:function(e,a,t){"use strict";t.r(a);var n=t(0),r=t.n(n),c=t(153),l=t(161),s=t(152);a.default=function(e){const{metadata:a,items:t}=e,{allTagsPath:n,name:m,count:o}=a;return r.a.createElement(c.a,{title:`Posts tagged "${m}"`,description:`Blog | Tagged "${m}"`},r.a.createElement("div",{className:"container margin-vert--lg"},r.a.createElement("div",{className:"row"},r.a.createElement("main",{className:"col col--8 col--offset-2"},r.a.createElement("h1",null,o," ",function(e,a){return e>1?a+"s":a}(o,"post"),' tagged with "',m,'"'),r.a.createElement(s.a,{href:n},"View All Tags"),r.a.createElement("div",{className:"margin-vert--xl"},t.map(({content:e})=>r.a.createElement(l.a,{key:e.metadata.permalink,frontMatter:e.frontMatter,metadata:e.metadata,truncated:!0},r.a.createElement(e,null))))))))}}}]);