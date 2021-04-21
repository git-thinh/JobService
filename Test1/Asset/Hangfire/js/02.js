﻿// https://github.com/sdecima/javascript-detect-element-resize/
!function(){function resetTriggers(element){var triggers=element.__resizeTriggers__,expand=triggers.firstElementChild,contract=triggers.lastElementChild,expandChild=expand.firstElementChild;contract.scrollLeft=contract.scrollWidth,contract.scrollTop=contract.scrollHeight,expandChild.style.width=expand.offsetWidth+1+"px",expandChild.style.height=expand.offsetHeight+1+"px",expand.scrollLeft=expand.scrollWidth,expand.scrollTop=expand.scrollHeight}function checkTriggers(element){return element.offsetWidth!=element.__resizeLast__.width||element.offsetHeight!=element.__resizeLast__.height}function scrollListener(e){var element=this;resetTriggers(this),this.__resizeRAF__&&cancelFrame(this.__resizeRAF__),this.__resizeRAF__=requestFrame(function(){checkTriggers(element)&&(element.__resizeLast__.width=element.offsetWidth,element.__resizeLast__.height=element.offsetHeight,element.__resizeListeners__.forEach(function(fn){fn.call(element,e)}))})}function createStyles(){if(!stylesCreated){var css=(animationKeyframes?animationKeyframes:"")+".resize-triggers { "+(animationStyle?animationStyle:"")+'visibility: hidden; opacity: 0; } .resize-triggers, .resize-triggers > div, .contract-trigger:before { content: " "; display: block; position: absolute; top: 0; left: 0; height: 100%; width: 100%; overflow: hidden; } .resize-triggers > div { background: #eee; overflow: auto; } .contract-trigger:before { width: 200%; height: 200%; }',head=document.head||document.getElementsByTagName("head")[0],style=document.createElement("style");style.type="text/css",style.styleSheet?style.styleSheet.cssText=css:style.appendChild(document.createTextNode(css)),head.appendChild(style),stylesCreated=!0}}var attachEvent=document.attachEvent,stylesCreated=!1;if(!attachEvent){var requestFrame=function(){var raf=window.requestAnimationFrame||window.mozRequestAnimationFrame||window.webkitRequestAnimationFrame||function(fn){return window.setTimeout(fn,20)};return function(fn){return raf(fn)}}(),cancelFrame=function(){var cancel=window.cancelAnimationFrame||window.mozCancelAnimationFrame||window.webkitCancelAnimationFrame||window.clearTimeout;return function(id){return cancel(id)}}(),animation=!1,animationstring="animation",keyframeprefix="",animationstartevent="animationstart",domPrefixes="Webkit Moz O ms".split(" "),startEvents="webkitAnimationStart animationstart oAnimationStart MSAnimationStart".split(" "),pfx="",elm=document.createElement("fakeelement");if(void 0!==elm.style.animationName&&(animation=!0),animation===!1)for(var i=0;i<domPrefixes.length;i++)if(void 0!==elm.style[domPrefixes[i]+"AnimationName"]){pfx=domPrefixes[i],animationstring=pfx+"Animation",keyframeprefix="-"+pfx.toLowerCase()+"-",animationstartevent=startEvents[i],animation=!0;break}var animationName="resizeanim",animationKeyframes="@"+keyframeprefix+"keyframes "+animationName+" { from { opacity: 0; } to { opacity: 0; } } ",animationStyle=keyframeprefix+"animation: 1ms "+animationName+"; "}window.addResizeListener=function(element,fn){attachEvent?element.attachEvent("onresize",fn):(element.__resizeTriggers__||("static"==getComputedStyle(element).position&&(element.style.position="relative"),createStyles(),element.__resizeLast__={},element.__resizeListeners__=[],(element.__resizeTriggers__=document.createElement("div")).className="resize-triggers",element.__resizeTriggers__.innerHTML='<div class="expand-trigger"><div></div></div><div class="contract-trigger"></div>',element.appendChild(element.__resizeTriggers__),resetTriggers(element),element.addEventListener("scroll",scrollListener,!0),animationstartevent&&element.__resizeTriggers__.addEventListener(animationstartevent,function(e){e.animationName==animationName&&resetTriggers(element)})),element.__resizeListeners__.push(fn))},window.removeResizeListener=function(element,fn){attachEvent?element.detachEvent("onresize",fn):(element.__resizeListeners__.splice(element.__resizeListeners__.indexOf(fn),1),element.__resizeListeners__.length||(element.removeEventListener("scroll",scrollListener),element.__resizeTriggers__=!element.removeChild(element.__resizeTriggers__)))}}();