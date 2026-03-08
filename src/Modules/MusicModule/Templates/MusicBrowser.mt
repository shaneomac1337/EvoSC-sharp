<component>
    <import component="EvoSC.Containers.Window" as="Window" />
    <import component="EvoSC.Style.UIStyle" as="UIStyle" />

    <property type="IEnumerable<dynamic>" name="songs" />
    <property type="IEnumerable<dynamic>" name="queue" />
    <property type="dynamic" name="currentSong" />

    <template>
        <UIStyle />
        <Window title="Music Browser" width="120" height="80" canClose="true">
            <template slot="body">
                <!-- Current Song -->
                <frame pos="0 0">
                    <label pos="2 -1" size="60 4" text="Now Playing:" textsize="1.5"
                           textcolor="FFFFFFAA" textfont="GameFontSemiBold" />
                    <label pos="2 -5" size="80 4"
                           text="{{ currentSong != null ? currentSong.Title + &quot; - &quot; + currentSong.Artist : &quot;Nothing playing&quot; }}"
                           textsize="1.5" textcolor="FFFFFFFF" />
                </frame>

                <!-- Queue Section -->
                <frame pos="0 -12">
                    <label pos="2 0" size="60 4" text="Queue:" textsize="1.5"
                           textcolor="FFFFFFAA" textfont="GameFontSemiBold" />
                    <label if="!queue.Any()" pos="2 -4" size="80 3" text="Queue is empty"
                           textsize="1" textcolor="FFFFFF88" />
                </frame>

                <!-- Song List Header -->
                <frame pos="0 -24">
                    <quad size="116 5" bgcolor="FFFFFF11" />
                    <label pos="2 -1" size="8 3" text="ID" textsize="1" textcolor="FFFFFFAA" />
                    <label pos="12 -1" size="45 3" text="Title" textsize="1" textcolor="FFFFFFAA" />
                    <label pos="58 -1" size="30 3" text="Artist" textsize="1" textcolor="FFFFFFAA" />
                    <label pos="90 -1" size="20 3" text="Action" textsize="1" textcolor="FFFFFFAA" />
                </frame>

                <!-- Song List -->
                <frame pos="0 -30">
                    <frame foreach="dynamic song in songs" pos="0 {{ -__index * 5 }}">
                        <quad size="116 5" bgcolor="{{ __index % 2 == 0 ? &quot;FFFFFF08&quot; : &quot;FFFFFF04&quot; }}" />
                        <label pos="2 -1" size="8 3" text="{{ song.Id }}" textsize="1" textcolor="FFFFFFFF" />
                        <label pos="12 -1" size="45 3" text="{{ song.Title }}" textsize="1" textcolor="FFFFFFFF" />
                        <label pos="58 -1" size="30 3" text="{{ song.Artist }}" textsize="1" textcolor="FFFFFFCC" />
                        <label pos="90 -1" size="20 3" text="Request"
                               textsize="1" textcolor="0F0FFFEE"
                               action="MusicModule.MusicManialinkController/RequestFromBrowserAsync/{{ song.Id }}"
                               focusareacolor1="00000000" focusareacolor2="FFFFFF22" />
                    </frame>
                </frame>
            </template>
        </Window>
    </template>
</component>
