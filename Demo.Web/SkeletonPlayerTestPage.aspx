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
				//file: 'http://cdn1.cache.twofourdigital.net/Mediafreedom/Storage/origin/amyprosser/da1908af-3316-4884-b064-9faa00ccb193/video/en/smoothstream/0631e6c4-3b4f-401f-a561-36ed109974d5.ism',
				height: 480,
				width: 853,
				skin: '/ExampleSkins/beelden.zip',
				playlist: [
					{
						duration: 301.96,
						title: "Road Warriors",
						file: 'http://cdn1.cache.twofourdigital.net/Mediafreedom/Storage/origin/amyprosser/da1908af-3316-4884-b064-9faa00ccb193/video/en/smoothstream/0631e6c4-3b4f-401f-a561-36ed109974d5.ism',
						image: '/thumbnail.jpg',
						captions: 'http://localhost:49832/captions_tt.xml',
						provider: 'http'
					},
					{ start: 2, duration: 6, file: "/video.mp4", image: "/thumbnail.jpg", title: "Item", description : "This is a little test video" },
					{ file: "/video.mp4", image: "/thumbnail.jpg", title: "Item",
							description : "This is a little test video with a very long description, is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum."  },
					{ file: "/video.mp4", title: "Item, with no thumb" },
					{ file: "/video.mp4", image: "/thumbnail.jpg", title: "Item" },
					{ file: "/video.mp4", image: "/thumbnail.jpg", title: "Item" },
					{ file: "/video.mp4", image: "/thumbnail.jpg", title: "Item" },
					{ file: "/video.mp4", image: "/thumbnail.jpg", title: "Item" },
					{ file: "/video.mp4", image: "/thumbnail.jpg", title: "Item" },
					{ file: "/video.mp4", image: "/thumbnail.jpg", title: "Item" },
					{ file: "/video.mp4", image: "/thumbnail.jpg", title: "Item" },
					{ file: "/video.mp4", image: "/thumbnail.jpg", title: "Item" },
					{ file: "/video.mp4", image: "/thumbnail.jpg", title: "Item" },
					{ file: "/video.mp4", image: "/thumbnail.jpg", title: "Item" },
					{ file: "/video.mp4", image: "/thumbnail.jpg", title: "Last One" }
				],
				"playlist.position": "right",
				"playlist.size": 300,
				modes: [
					{ type: 'silverlight', src: '/ClientBin/jwslPlayer.xap' },
					{ type: 'flash', src: '/ClientBin/player.swf' },
					{ type: 'html5' },
					{ type: 'download' }
				]
			});
		</script>


		<!-- Test functions: -->
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
		<a href="#" onclick="jwplayer().playlistItem(0)">First Playlist Item</a> |
		<a href="#" onclick="jwplayer().playlistNext()">Next Playlist Item</a> |
		<a href="#" onclick="jwplayer().playlistPrev()">Prev Playlist Item</a> |
		<a href="#" onclick="jwplayer().load([ { file: '/video.mp4', image: '/thumbnail.jpg', title: 'One' }, { file: '/video.mp4', image: '/thumbnail.jpg', title: 'Two' }, { file: '/video.mp4', image: '/thumbnail.jpg', title: 'Three' }])">Set New Playlist</a> |
		<a href="#" onclick="jwplayer().load({ file: '/video.mp4', image: '/thumbnail.jpg', title: 'Single' })">Set single playlist item </a> |
		<a href="#" onclick="jwplayer().load('/video.mp4')">Set single item </a>
		<br />
		<a href="#" onclick="jwplayer().getPlugin('controlbar').show()">Show Control Bar</a> |
		<a href="#" onclick="jwplayer().getPlugin('controlbar').hide()">Hide Control Bar</a>

	</form>
</body>
</html>
