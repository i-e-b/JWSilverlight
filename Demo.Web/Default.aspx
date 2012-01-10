<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>SkeletonPlayer</title>
    <style type="text/css"> html, body { height: 100%; overflow: auto; } body { padding: 0; margin: 0; } </style>
</head>
<body>
	<form id="form1" runat="server"> 
		<!-- Target div for player: -->
		<div id="container">Loading the player...</div>

		<!-- embed includes and call -->
		<script type="text/javascript" src="/Scripts/jwplayer.min.js"></script>
		<script type="text/javascript" src="/Scripts/jwplayer.embed.flash.js"></script><!-- to fix an issue, remove later -->
		<script type="text/javascript" src="/Scripts/jwplayer.embed.silverlight.js"></script>
		<script type="text/javascript">
			jwplayer('container').setup({
				height: 480,
				width: 853,
				skin: '/ExampleSkins/glow_with_html5/glow.zip',
				playlist: [
					{
						duration: 596,
						title: "Big Buck Bunny",
						file: 'http://mediadl.microsoft.com/mediadl/iisnet/smoothmedia/Experience/BigBuckBunny_720p.ism',
						image: '/thumbnail.jpg',
						captions: 'http://localhost:49832/captions_tt.xml'
					},
					{ start: 2, duration: 6, file: "/video.mp4", image: "/thumbnail.jpg", title: "JW Player Standard Sample Video", description : "This is a little test video" },
				],
				/*"playlist.position": "right",
				"playlist.size": 300,*/
				plugins: 'captions-2',
				modes: [
					{ type: 'silverlight', src: '/ClientBin/jwslPlayer.xap' },
					{ type: 'flash', src: '/ClientBin/player.swf' },
					{ type: 'html5' },
					{ type: 'download' }
				]
			});
		</script>

		<!-- Test functions: -->
		<div id="Test-Functions" style="font-size:small">
		<a href="#" onclick="jwplayer().stop()">Stop</a> |
		<a href="#" onclick="jwplayer().pause(false)">Pause</a> |
		<a href="#" onclick="jwplayer().play()">Toggle Play</a> |
		<a href="#" onclick="alert(jwplayer().getState())">Get Player State</a> |
		<a href="#" onclick="jwplayer().seek(30)">Go to 30 Seconds</a>
		<br />
		<a href="#" onclick="alert(jwplayer().getPosition())">Get Position</a> |
		<a href="#" onclick="alert(jwplayer().getDuration())">Get Duration</a> |
		<a href="#" onclick="alert(jwplayer().getBuffer()+'%')">Get Buffer Progress</a> (always shows 0% for smooth streams)
		<br />
		<a href="#" onclick="alert(jwplayer().getFullscreen())">Is Fullscreen?</a> |
		<a href="#" onclick="jwplayer().setFullscreen()">Toggle Fullscreen (HTML5 Only)</a> |
		<a href="#" onclick="alert(jwplayer().getVolume())">Get Volume</a> |
		<a href="#" onclick="jwplayer().setVolume(50)">Set Half Volume</a> |
		<a href="#" onclick="jwplayer().setVolume(100)">Set Full Volume</a> |
		<a href="#" onclick="jwplayer().setMute()">Toggle Mute</a>
		<br />
		<a href="#" onclick="alert(jwplayer().getWidth())">Get Width</a> |
		<a href="#" onclick="alert(jwplayer().getHeight())">Get Height</a> |
		<a href="#" onclick="jwplayer().resize(480,270)">Make Small</a> |
		<a href="#" onclick="jwplayer().resize(853,480)">Make Big</a>
		<br />
		<a href="#" onclick="alert(jwplayer().getMeta())">Get Meta</a> |
		<a href="#" onclick="alert(jwplayer().getPlaylist())">Get Playlist</a> |
		<a href="#" onclick="jwplayer().playlistItem(0)">First Playlist Item</a> |
		<a href="#" onclick="jwplayer().playlistNext()">Next Playlist Item</a> |
		<a href="#" onclick="jwplayer().playlistPrev()">Prev Playlist Item</a> |
		<a href="#" onclick="jwplayer().load([ { file: '/video.mp4', image: '/thumbnail.jpg', title: 'One' }, { file: '/video.mp4', image: '/thumbnail.jpg', title: 'Two' }, { file: '/video.mp4', image: '/thumbnail.jpg', title: 'Three' }])">Set New Playlist</a> |
		<a href="#" onclick="jwplayer().load({ file: '/video.mp4', image: '/thumbnail.jpg', title: 'Single' })">Set single playlist item </a> |
		<a href="#" onclick="jwplayer().load('/video.mp4')">Set single item </a>
		<br />
		<a href="#" onclick="jwplayer().getPlugin('controlbar').show()">Show Control Bar</a> |
		<a href="#" onclick="jwplayer().getPlugin('controlbar').hide()">Hide Control Bar</a> |
		<a href="#" onclick="jwplayer().getPlugin('dock').show()">Show Dock Buttons</a> |
		<a href="#" onclick="jwplayer().getPlugin('dock').hide()">Hide Dock Buttons</a> |
		<a href="#" onclick="jwplayer().getPlugin('display').show()">Show Display Box</a> |
		<a href="#" onclick="jwplayer().getPlugin('display').hide()">Hide Display Box</a>
		<br />
		<a href="#" onclick="jwplayer().getPlugin('dock').setButton('tstbtn', function(){alert('button pushed');}, '/Images/DemoButton.png', '/Images/DemoButtonOver.png' )">Add Plugin Button</a> |
		<a href="#" onclick="jwplayer().getPlugin('dock').setButton('tstbtn')">Remove Plugin Button</a>
		<br /></div>

		<!-- Test event code: -->
		<script type="text/javascript">
			function setText(text) {
				document.getElementById("message").innerHTML += text;
			}
			function setStatus(text) {
				document.getElementById("status").innerHTML = text;
			}

			jwplayer().onBuffer(function () { setText("Buffering; "); });
			jwplayer().onBufferChange(function (o) { setStatus("Buffering " + o.bufferPercent + "%"); });
			jwplayer().onBufferFull(function () { setText("Buffer full; "); });
			jwplayer().onComplete(function () { setText("Playback complete; "); });
			jwplayer().onError(function (o) { setText("Error " + o.message + "; "); });
			jwplayer().onFullscreen(function (o) { setText("Fullscreen " + o.fullscreen + "; "); });
			jwplayer().onIdle(function () { setText("Idle; "); });
			jwplayer().onMeta(function (o) { setText("Metadata; "); });
			jwplayer().onMute(function (o) { setText("Mute: " + o.mute + "; "); });
			jwplayer().onPause(function () { setText("Paused; "); });
			jwplayer().onPlay(function () { setText("Playing; "); });
			jwplayer().onPlaylist(function (o) { setText("Loaded playlist; "); });
			jwplayer().onPlaylistItem(function (o) { setText(o.index + "; "); });
			jwplayer().onReady(function () { setText("Ready; "); });
			jwplayer().onResize(function (o) { setText("Resized: " + o.width + "x" + o.height + "; "); });
			jwplayer().onSeek(function (o) { setText("Moved to " + o.offset + " from " + o.position + "; "); });
			jwplayer().onTime(function (o) { setStatus("Update, duration: " + o.duration + ", seeking: " + o.offset + ", position:" + o.position); });
			jwplayer().onVolume(function (o) { setText("Volume "+o.volume+"%; "); });

			jwplayer().getPlugin('controlbar').onHide(function (o) { setText("Control bar hidden; "); });
			jwplayer().getPlugin('controlbar').onShow(function (o) { setText("Control bar shown; "); });
			jwplayer().getPlugin('dock').onHide(function (o) { setText("Dock hidden; "); });
			jwplayer().getPlugin('dock').onShow(function (o) { setText("Dock shown; "); });
			jwplayer().getPlugin('display').onHide(function (o) { setText("Display hidden; "); });
			jwplayer().getPlugin('display').onShow(function (o) { setText("Display shown; "); });
		</script>
		
		<!-- Event messages -->
		<p id="status"></p>
		<p id="message"></p>
	</form>
</body>
</html>
