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
    <script type="text/javascript" src="Silverlight.js"></script>
    <script type="text/javascript">

        function showTextCaption(captionString)
        {
            alert(captionString);
        }


        function onSilverlightError(sender, args) {
            var appSource = "";
            if (sender != null && sender != 0) {
              appSource = sender.getHost().Source;
            }
            
            var errorType = args.ErrorType;
            var iErrorCode = args.ErrorCode;

            if (errorType == "ImageError" || errorType == "MediaError") {
              return;
            }

            var errMsg = "Unhandled Error in Silverlight Application " +  appSource + "\n" ;

            errMsg += "Code: "+ iErrorCode + "    \n";
            errMsg += "Category: " + errorType + "       \n";
            errMsg += "Message: " + args.ErrorMessage + "     \n";

            if (errorType == "ParserError") {
                errMsg += "File: " + args.xamlFile + "     \n";
                errMsg += "Line: " + args.lineNumber + "     \n";
                errMsg += "Position: " + args.charPosition + "     \n";
            }
            else if (errorType == "RuntimeError") {           
                if (args.lineNumber != 0) {
                    errMsg += "Line: " + args.lineNumber + "     \n";
                    errMsg += "Position: " +  args.charPosition + "     \n";
                }
                errMsg += "MethodName: " + args.methodName + "     \n";
            }

            throw new Error(errMsg);
        }
    </script>
</head>
<body>
    <form id="form1" runat="server" style="height:100%">
    <div id="silverlightControlHost">
        <object id="slPlugin" data="data:application/x-silverlight-2," type="application/x-silverlight-2" width="100%" height="100%">
		  <param name="source" value="ClientBin/jwslPlayer.xap"/>
		  <param name="onError" value="onSilverlightError" />
		  <param name="background" value="white" />
		  <param name="minRuntimeVersion" value="3.0.40624.0" />
		  <param name="autoUpgrade" value="true" />
		<param name="initParams" value="title=My%20Player%20Stats,playlist=http://localhost:49832/Playlist.xml,captionMethod=showTextCaption" />
		
      
        <!--  <param name="initParams" value="playlist=%3C%3Fxml%20version%3D%221.0%22%20encoding%3D%22utf-8%22%20%3F%3E%3CPlaylist%3E%3CAutoLoad%3Etrue%3C%2FAutoLoad%3E%3CAutoPlay%3Etrue%3C%2FAutoPlay%3E%3CDisplayTimeCode%3Efalse%3C%2FDisplayTimeCode%3E%3CEnableCachedComposition%3Etrue%3C%2FEnableCachedComposition%3E%3CEnableCaptions%3Etrue%3C%2FEnableCaptions%3E%3CEnablePopOut%3Etrue%3C%2FEnablePopOut%3E%3CEnableOffline%3Efalse%3C%2FEnableOffline%3E%3CStartMuted%3Efalse%3C%2FStartMuted%3E%3CStretchMode%3EUniform%3C%2FStretchMode%3E%3CItems%3E%3CPlaylistItem%3E%3CTitle%3Etest%3C%2FTitle%3E%3CDescription%3Etest%3C%2FDescription%3E%3CHeight%3E258%3C%2FHeight%3E%3CWidth%3E460%3C%2FWidth%3E%3CIsAdaptiveStreaming%3Etrue%3C%2FIsAdaptiveStreaming%3E%3COfflineVideoBitrateInKbps%3E0%3C%2FOfflineVideoBitrateInKbps%3E%3CMediaSource%3Ehttp%3A%2F%2Fparliament.smooth.twofourdigital.net%2FUKP_HCS_6436.isml%2FMANIFEST%3C%2FMediaSource%3E%3CResume%3E0%3C%2FResume%3E%3CBaseTime%3E2010-07-13%2014%3A10%3A01%3C%2FBaseTime%3E%3CCaptionUrl%3Ehttp%3A%2F%2Flocalhost%3A51861%2FHandlers%2FCaptions.ashx%3FMeetingId%3D6436%3C%2FCaptionUrl%3E%3C%2FPlaylistItem%3E%3C%2FItems%3E%3C%2FPlaylist%3E,captionMethod=showTextCaption" />
		-->   <a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=3.0.40624.0" style="text-decoration:none">
 			  <img src="http://go.microsoft.com/fwlink/?LinkId=108181" alt="Get Microsoft Silverlight" style="border-style:none"/>
		  </a>
	    </object><iframe id="_sl_historyFrame" style="visibility:hidden;height:0px;width:0px;border:0px"></iframe></div>
    </form>
</body>
</html>
