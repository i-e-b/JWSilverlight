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
		<script type="text/javascript" src="/Scripts/jwplayer.embed.silverlight.js"></script>
		<script type="text/javascript">
			jwplayer('container').setup({
				//file: 'http://cdn1.cache.twofourdigital.net/Mediafreedom/Storage/origin/amyprosser/da1908af-3316-4884-b064-9faa00ccb193/video/en/smoothstream/0631e6c4-3b4f-401f-a561-36ed109974d5.ism/MANIFEST',
				height: 480,
				width: 853,
				skin: '/ExampleSkins/glow.zip',
				playlist: [
					{
						//duration: 32,
						title: "Road Warriors",
						file: 'http://cdn1.cache.twofourdigital.net/Mediafreedom/Storage/origin/amyprosser/da1908af-3316-4884-b064-9faa00ccb193/video/en/smoothstream/0631e6c4-3b4f-401f-a561-36ed109974d5.ism/MANIFEST',
						image: '/thumbnail.jpg',
						captions: 'http://localhost:49832/captions_tt.xml'
					},
					{ file: "/video.mp4", image: "/thumbnail.jpg", title:"Second"},
					{ file: "/video.mp4", image: "/thumbnail.jpg", title: "Third" }
				],
				"playlist.position": "right",
				"playlist.size": 360,
				modes: [
					{ type: 'silverlight', src: '/ClientBin/jwslPlayer.xap' },
					{ type: 'flash', src: '/ClientBin/player.swf' },
					{ type: 'html5' },
					{ type: 'download' }
				]
			});
		</script>


		<!-- Test functions: -->
		<a href="#" onclick="jwplayer().stop()">Stop</a>
		<a href="#" onclick="jwplayer().pause(false)">Pause</a>
		<a href="#" onclick="jwplayer().play()">Toggle Play</a>
		<a href="#" onclick="alert(jwplayer().getState())">Get Player State</a>
		<a href="#" onclick="jwplayer().seek(30)">Go to 30 Seconds</a>
		<br />
		<a href="#" onclick="alert(jwplayer().getPosition())">Get Position</a>
		<a href="#" onclick="alert(jwplayer().getDuration())">Get Duration</a>
		<a href="#" onclick="alert(jwplayer().getBuffer()+'%')">Get Buffer Progress</a> (always shows 0% for smooth streams)
		<br />
		<a href="#" onclick="alert(jwplayer().getFullscreen())">Is Fullscreen?</a>
		<a href="#" onclick="jwplayer().setFullscreen()">Toggle Fullscreen (HTML5 Only)</a>
		<a href="#" onclick="alert(jwplayer().getVolume())">Get Volume</a>
		<a href="#" onclick="jwplayer().setVolume(50)">Set Half Volume</a>
		<a href="#" onclick="jwplayer().setVolume(100)">Set Full Volume</a>
		<a href="#" onclick="jwplayer().setMute()">Toggle Mute</a>
		<br />
		<a href="#" onclick="alert(jwplayer().getWidth())">Get Width</a>
		<a href="#" onclick="alert(jwplayer().getHeight())">Get Height</a>

	</form>
</body>
</html>
