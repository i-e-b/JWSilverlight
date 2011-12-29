<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>SkeletonPlayer</title>
    <style type="text/css">
    html, body {
	    height: 100%;
	    overflow: auto;
    }
    body {
	    padding: 0;
	    margin: 0;
    }
    #silverlightControlHost {
	    height: 100%;
	    text-align:center;
    }
    </style>
</head>
<body>
	<form id="form1" runat="server">
		<!-- <div id="silverlightControlHost">
			<object id="slPlugin" data="data:application/x-silverlight-2," type="application/x-silverlight-2" width="853" height="480">
				<param name="source" value="ClientBin/jwslPlayer.xap" />
				<param name="onError" value="onSilverlightError" />
				<param name="background" value="white" />
				<param name="minRuntimeVersion" value="3.0.40624.0" />
				<param name="autoUpgrade" value="true" />
				<param name="initParams" value="playlist=http://localhost:49832/Playlist.xml,skin=/ExampleSkins/glow.zip" />
				<a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=3.0.40624.0" style="text-decoration: none">
					<img src="http://go.microsoft.com/fwlink/?LinkId=108181" alt="Get Microsoft Silverlight" style="border-style: none" />
				</a>
			</object>
			<iframe id="_sl_historyFrame" style="visibility: hidden; height: 0px; width: 0px; border: 0px"></iframe>
		</div> -->


		<div id="container">Loading the player...</div>

		<a href="#" onclick="Pause()">Pause</a>
		<a href="#" onclick="Play()">Play</a>

		<script type="text/javascript" src="/Scripts/jwplayer.min.js"></script>
		<script type="text/javascript" src="/Scripts/jwplayer.embed.silverlight.js"></script>
		<script type="text/javascript">
			jwplayer('container').setup({
				file: 'http://cdn1.cache.twofourdigital.net/Mediafreedom/Storage/origin/amyprosser/da1908af-3316-4884-b064-9faa00ccb193/video/en/smoothstream/0631e6c4-3b4f-401f-a561-36ed109974d5.ism/MANIFEST',
				height: 480,
				width: 853,
				modes: [
					{ type: 'silverlight', src: '/ClientBin/jwslPlayer.xap' },
				  { type: 'flash', src: 'player.swf' },
				  //{ type: 'html5' },
				  //{ type: 'download' }
				]
			});
			
			function Play() {
				// should be:
				// jwplayer().play()
				// but is:
				jwplayer().container.content.jwplayer.Play()
			}
			function Pause() {
				// should be:
				// jwplayer().pause()
				// but is:
				jwplayer().container.content.jwplayer.Pause()
			}
		</script>
	</form>
</body>
</html>
