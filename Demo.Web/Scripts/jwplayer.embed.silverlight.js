/**
 * Silverlight mode embedder for the JW Player
 * Adapted from jwplayer.embed.flash.js by Zach
 * @author Iain Ballard
 * @version 5.5
 */
(function (jwplayer) {
	// Bind new extensions that only Silverlight can handle:
	jwplayer.utils.extensionmap["ism"] = { "silverlight": "video/smooth" },
	jwplayer.utils.extensionmap["isml"]= { "silverlight": "video/smooth" },
	jwplayer.utils.extensionmap["wmv"] = { "silverlight": "video/windowsmedia" },
	jwplayer.utils.extensionmap["wma"] = { "silverlight": "audio/windowsmedia" },
	jwplayer.utils.extensionmap["avi"] = { "silverlight": "video/avi" },

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
			html += '<param name="windowless" value="' + wmode + '">';
			html += '<param name="initParams" value="' +
				jsonToFlashvars(params) +
				'">';
			html += '</object>';

			jwplayer.utils.setOuterHTML(_container, html);

			flashPlayer = document.getElementById(_container.id);
			_api.container = flashPlayer;
			_api.setPlayer(flashPlayer, "flash");
		}

		this.supportsConfig = function () {
			return true; // todo: Silverlight detection & file can play.
		}
	};


})(jwplayer);
