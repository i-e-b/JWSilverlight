Skins
-----
* playlist reading and rendering
* logo plugin rendering (link out)

Javascript API
--------------
* Expose javascript interface to player (replicate required calls in HTMLBridge, and modify jwplayer.api.js to match?
                                         if possible, try to bridge with embed plugin so modified api isn't needed.)
* compatible player loader (add a "jwplayer.embed.silverlight.js" to compliment "jwplayer.embed.[flash|html5].js",
  add a new default mode to modes config?)

Misc
----
* Cleanup or re-write ComposerCore for Smooth 1.5 player
* Background colour on poster view
* Keyboard commands
* Trace usage in ZIP library, exclude unused parts (for size)