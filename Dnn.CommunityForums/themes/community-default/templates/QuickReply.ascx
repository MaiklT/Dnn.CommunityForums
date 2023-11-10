<div class="dcf-quick-reply">
	<header>
		<h3 class="dcf-title dcf-title-3">[RESX:QuickReply]</h3>
		<!--<div class="dcf-collapse">[AF:CONTROLS:GROUPTOGGLE]</div> -->
	</header>
	<section class="dcf-section">
			<div class="dcf-group" id="groupQR">
				<asp:PlaceHolder ID="plhMessage" runat="server" />
				<div>
					<div class="dcf-qr-subject"> 
						<h4 class="dcf-title dcf-title-4">[RESX:Subject]:</h4> 
						<input type="text" id="txtSubject" class="dcf-textbox" readonly="readonly" value="[SUBJECT]" />
					</div> 
					<div class="dcf-qr-text">
						
						<asp:Label ID="reqBody" runat="server" Visible="false" />
												
						<h4 class="dcf-title dcf-title-4">[RESX:Body]:</h4>
						
							<div class="dcf-toolbar dcf-toolbar-buttons" id="btnToolBar" runat="server">	
							
								 
								<i class="fa fa-bold fa-fw" accesskey="b" onclick="insertCode('[b] [/b]');" onmouseover="window.status='[RESX:BoldDesc]'; return true;" onmouseout="window.status=''; return true;"></i>
								<i class="fa fa-italic fa-fw" accesskey="i" onclick="insertCode('[i] [/i]');" onmouseover="window.status='[RESX:ItalicsDesc]'; return true;" onmouseout="window.status=''; return true;"></i>
								<i class="fa fa-underline fa-fw" accesskey="u" onclick="insertCode('[u] [/u]');" onmouseover="window.status='[RESX:UnderlineDesc]'; return true;" onmouseout="window.status=''; return true;"></i>
								<i class="fa fa-quote-left fa-fw" accesskey="q" onclick="insertQuote();" onmouseover="window.status='[RESX:QuoteDesc]'; return true;" onmouseout="window.status=''; return true;"></i>
								<i class="fa fa-image fa-fw" accesskey="m" onclick="insertCode('[img] [/img]');" onmouseover="window.status='[RESX:ImageDesc]'; return true;" onmouseout="window.status=''; return true;"></i>
								<i class="fa fa-code fa-fw" accesskey="c" onclick="insertCode('[code] [/code]');" onmouseover="window.status='[RESX:CodeDesc]'; return true;" title='[RESX:CodeDesc]' onmouseout="window.status=''; return true;"></i>

							<!--
									<input type="button" class="dcf-Button" accesskey="b" name="dcf-Bold" value="[RESX:Bold]" style="font-weight:bold;" onclick="insertCode('[b] [/b]');" onmouseover="window.status='[RESX:BoldDesc]';  return true;" onmouseout="window.status=''; return true;" />
								<input type="button" class="dcf-Button" accesskey="i" name="dcf-Bold" value="[RESX:Italics]"  style="font-weight:bold;" onclick="insertCode('[i] [/i]');" onmouseover="window.status='[RESX:ItalicsDesc]';  return true;" onmouseout="window.status=''; return true;" />
								<input type="button" class="dcf-Button" accesskey="u" name="dcf-Bold" value="[RESX:Underline]" style="font-weight:bold;" onclick="insertCode('[u] [/u]');" onmouseover="window.status='[RESX:UnderlineDesc]';  return true;" onmouseout="window.status=''; return true;" />
								<input type="button" class="dcf-Button" accesskey="q" name="dcf-Bold" value="[RESX:Quote]" style="font-weight:bold;" onclick="insertQuote();" onmouseover="window.status='[RESX:QuoteDesc]';  return true;" onmouseout="window.status=''; return true;" />
								<input type="button" class="dcf-Button" accesskey="m" name="dcf-Bold" value="[RESX:Image]" style="font-weight:bold;" onclick="insertCode('[img] [/img]');" onmouseover="window.status='[RESX:ImageDesc]';  return true;" onmouseout="window.status=''; return true;" />
								<input type="button" class="dcf-Button" accesskey="c" name="dcf-Bold" value="[RESX:Code]" style="font-weight:bold;" onclick="insertCode('[code] [/code]');" onmouseover="window.status='[RESX:CodeDesc]';  return true;" title='[RESX:CodeDesc]' onmouseout="window.status=''; return true;" />
							-->
								
							</div>
							<textarea id="txtBody" name="txtBody" class="dcf-textbox" rows="5" cols="250"></textarea>
						</div>
					
					[AF:UI:ANON]
						<div class="dcf-qr-username">
							<div class="NormalBold">[RESX:Username]:[AF:REQ:USERNAME]</div>
							<div style="width:150px;">[AF:INPUT:USERNAME]</div>
							<div></div>
						</div>
						<div class="dcf-qr-securitycode">
							<div class="NormalBold">[RESX:SecurityCode]:[AF:REQ:SECURITYCODE]</div>
							<div>[AF:INPUT:CAPTCHA]</div> 
						</div>
					[/AF:UI:ANON]
					<div class="dcf-subscribe">
						<div id="divSubscribe" runat="server" />
					</div> 
					
				</div>
			</div>

	</section>
	<footer "dcf-footer">
	<div class="dcf-buttons">
		[AF:BUTTON:SUBMIT]
	</div>
	</footer>
</div>