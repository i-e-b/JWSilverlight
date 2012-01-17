/**
 * Silverlight mode embedder for the JW Player
 * Adapted from jwplayer.embed.flash.js by Zach
 * @author Iain Ballard
 * @version 5.5
 */
(function (jwplayer) {
	// Bind new extensions that only Silverlight can handle:
	jwplayer.utils.extensionmap["ism"] = { "silverlight": "video/smooth" },
	jwplayer.utils.extensionmap["isml"] = { "silverlight": "video/smooth" },
	jwplayer.utils.extensionmap["wmv"] = { "silverlight": "video/windowsmedia" },
	jwplayer.utils.extensionmap["wma"] = { "silverlight": "audio/windowsmedia" },
	jwplayer.utils.extensionmap["avi"] = { "silverlight": "video/avi" },

	// Bind into existing extensions that Silverlight can handle:
	jwplayer.utils.extensionmap["mp4"].silverlight = "video/mp4";
	jwplayer.utils.extensionmap["m4v"].silverlight = "video/mp4";

	jwplayer.embed.silverlight = function (_container, _player, _options, _loader, _api) {
		function appendAttribute(object, name, value) {
			var param = document.createElement('param');
			param.setAttribute('name', name);
			param.setAttribute('value', value);
			object.appendChild(param);
		};

		function _resizePlugin(plugin, div, onready) {
			return function (evt) {
				if (onready) {
					document.getElementById(_api.id + "_wrapper").appendChild(div);
				}
				var display = document.getElementById(_api.id).getPluginConfig("display");
				plugin.resize(display.width, display.height);
				var style = {
					left: display.x,
					top: display.y
				}
				jwplayer.utils.css(div, style);
			}
		}


		function parseComponents(componentBlock) {
			if (!componentBlock) {
				return {};
			}

			var flat = {};

			for (var component in componentBlock) {
				var componentConfig = componentBlock[component];
				for (var param in componentConfig) {
					flat[component + '.' + param] = componentConfig[param];
				}
			}

			return flat;
		};
		function axDetect(ax) {
			// adapted from http://www.silverlightversion.com/
			var v = [0, 0, 0, 0],
					loopMatch = function (ax, v, i, n) {
						while (ax.isVersionSupported(v[0] + "." + v[1] + "." + v[2] + "." + v[3])) {
							v[i] += n;
						}
						v[i] -= n;
					};
			loopMatch(ax, v, 0, 1);
			loopMatch(ax, v, 1, 1);
			loopMatch(ax, v, 2, 10000);
			loopMatch(ax, v, 2, 1000);
			loopMatch(ax, v, 2, 100);
			loopMatch(ax, v, 2, 10);
			loopMatch(ax, v, 2, 1);
			loopMatch(ax, v, 3, 1);

			return v;
		};

		function detectPlugin(pluginName, mimeType, activeX) {
			var nav = window.navigator;
			var version = [0, 0, 0], description, i, ax;

			// Firefox, Webkit, Opera
			if (typeof (nav.plugins) != 'undefined' && typeof nav.plugins[pluginName] == 'object') {
				description = nav.plugins[pluginName].description;
				if (description && !(typeof nav.mimeTypes != 'undefined' && nav.mimeTypes[mimeType] && !nav.mimeTypes[mimeType].enabledPlugin)) {
					version = description.replace(pluginName, '').replace(/^\s+/, '').replace(/\sr/gi, '.').split('.');
					for (i = 0; i < version.length; i++) {
						version[i] = parseInt(version[i].match(/\d+/), 10);
					}
				}
				// Internet Explorer / ActiveX
			} else if (typeof (window.ActiveXObject) != 'undefined') {
				try {
					ax = new ActiveXObject(activeX);
					if (ax) { version = axDetect(ax); }
				} catch (e) { }
			}
			return version;
		};
		function hasSilverlight() {
			var slversion = detectPlugin('Silverlight Plug-In', 'application/x-silverlight-2', 'AgControl.AgControl');
			if (slversion[0] < 3) return false;
			return true;
		}

		function parseConfigBlock(options, blockName) {
			if (options[blockName]) {
				var components = options[blockName];
				for (var name in components) {
					var component = components[name];
					if (typeof component == "string") {
						// i.e. controlbar="over"
						if (!options[name]) {
							options[name] = component;
						}
					} else {
						// i.e. controlbar.position="over"
						for (var option in component) {
							if (!options[name + '.' + option]) {
								options[name + '.' + option] = component[option];
							}
						}
					}
				}
				delete options[blockName];
			}
		};

		function parsePlugins(pluginBlock) {
			if (!pluginBlock) {
				return {};
			}

			var flat = {}, pluginKeys = [];

			for (var plugin in pluginBlock) {
				var pluginName = jwplayer.utils.getPluginName(plugin);
				var pluginConfig = pluginBlock[plugin];
				pluginKeys.push(plugin);
				for (var param in pluginConfig) {
					flat[pluginName + '.' + param] = pluginConfig[param];
				}
			}
			flat.plugins = pluginKeys.join(',');
			return flat;
		};

		function jsonToFlashvars(json) {
			var flashvars = json.netstreambasepath ? '' : 'netstreambasepath=' + encodeURIComponent(window.location.href.split("#")[0]) + ', ';
			for (var key in json) {
				if (typeof (json[key]) == "object") {
					flashvars += key + '=' + encodeURIComponent("[[JSON]]" + jwplayer.utils.strings.jsonToString(json[key])) + ', ';
				} else {
					flashvars += key + '=' + encodeURIComponent(json[key]) + ', ';
				}
			}
			return flashvars.substring(0, flashvars.length - 1);
		};

		this.embed = function () {
			// Make sure we're passing the correct ID into Flash for Linux API support
			_options.id = _api.id;

			var _wrapper;

			var params = jwplayer.utils.extend({}, _options);

			var width = params.width;
			var height = params.height;

			// Hack for when adding / removing happens too quickly
			if (_container.id + "_wrapper" == _container.parentNode.id) {
				_wrapper = document.getElementById(_container.id + "_wrapper");
			} else {
				_wrapper = document.createElement("div");
				_wrapper.id = _container.id + "_wrapper";
				jwplayer.utils.wrap(_container, _wrapper);
				jwplayer.utils.css(_wrapper, {
					position: "relative",
					width: width,
					height: height
				});
			}


			var flashPlugins = _loader.setupPlugins(_api, params, _resizePlugin);

			if (flashPlugins.length > 0) {
				jwplayer.utils.extend(params, parsePlugins(flashPlugins.plugins));
			} else {
				delete params.plugins;
			}


			var toDelete = ["height", "width", "modes", "events"];

			for (var i = 0; i < toDelete.length; i++) {
				delete params[toDelete[i]];
			}

			var wmode = "false";
			if (params.wmode) {
				wmode = params.wmode; // todo: translate modes to either true or false?
			}

			parseConfigBlock(params, 'components');
			parseConfigBlock(params, 'providers');

			// Hack for the dock
			if (typeof params["dock.position"] != "undefined") {
				if (params["dock.position"].toString().toLowerCase() == "false") {
					params["dock"] = params["dock.position"];
					delete params["dock.position"];
				}
			}

			var bgcolor = "#000000";

			var flashPlayer;
			var html = '<object data="data:application/x-silverlight-2," type="application/x-silverlight-2" ' +
				'bgcolor="' +
				bgcolor +
				'" width="100%" height="100%" ' +
				'id="' +
				_container.id +
				'" name="' +
				_container.id +
				'" tabindex=0"' +
				'">';
			html += '<param name="source" value="' + _player.src + '">';
			html += '<param name="background" value="' + bgcolor + '">';
			html += '<param name="minRuntimeVersion" value="3.0.40624.0" />';
			html += '<param name="autoUpgrade" value="true" />';
			html += '<param name="enablehtmlaccess" value="True"/>';
			html += '<param name="windowless" value="' + wmode + '">';
			html += '<param name="initParams" value="' +
				jsonToFlashvars(params) +
				'">';
			html += '</object>';

			jwplayer.utils.setOuterHTML(_container, html);

			flashPlayer = document.getElementById(_container.id);
			_api.container = flashPlayer;
			_api.setPlayer(flashPlayer, "flash");
		};

		this.supportsConfig = function () {
			if (hasSilverlight()) {
				if (_options) {
					var item = jwplayer.utils.getFirstPlaylistItemFromConfig(_options);
					if (typeof item.file == "undefined" && typeof item.levels == "undefined") {
						return true;
					} else if (item.file) {
						return silverlightCanPlay(item.file, item.provider);
					} else if (item.levels && item.levels.length) {
						for (var i = 0; i < item.levels.length; i++) {
							if (item.levels[i].file && silverlightCanPlay(item.levels[i].file, item.provider)) {
								return true;
							}
						}
					}
				} else {
					return true;
				}
			}
			return false;
		}

		silverlightCanPlay = function (file, provider) {
			var extension = jwplayer.utils.extension(file);
			// If there is no extension, use Flash
			if (!extension) {
				return false;
			}
			// Extension is in the extension map, but not supported by Silverlight - fail
			if (jwplayer.utils.exists(jwplayer.utils.extensionmap[extension]) &&
					!jwplayer.utils.exists(jwplayer.utils.extensionmap[extension].silverlight)) {
				return false;
			}

			return true;
		};
	};


})(jwplayer);
