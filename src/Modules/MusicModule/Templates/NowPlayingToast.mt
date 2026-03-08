<component>
    <import component="EvoSC.Style.UIStyle" as="UIStyle" />

    <property type="string" name="title" />
    <property type="string" name="artist" />

    <template>
        <UIStyle />
        <frame pos="140 -75" id="now_playing_frame">
            <quad size="50 10" bgcolor="000000AA" style="Bgs1" substyle="BgDialogBlur" />
            <quad pos="1 -1" size="2 8" bgcolor="0F0FFFAA" />
            <label pos="5 -1.5" size="43 4" text="Now Playing" textsize="1"
                   textcolor="FFFFFFAA" />
            <label pos="5 -5" size="43 4" text="{{ title }}" textsize="1.5"
                   textcolor="FFFFFFFF" textfont="GameFontSemiBold" />
            <label pos="5 -8" size="43 3" text="{{ artist }}" textsize="1"
                   textcolor="FFFFFFAA" />
        </frame>
    </template>

    <script>
        <!--
        main() {
            declare frame = (Page.GetFirstChild("now_playing_frame") as CMlFrame);
            declare Real startTime = Now / 1000.0;

            while(True) {
                yield;
                declare Real elapsed = (Now / 1000.0) - startTime;
                if (elapsed > 5.0) {
                    frame.Hide();
                    break;
                }
            }
        }
        -->
    </script>
</component>
