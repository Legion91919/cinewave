#!/usr/bin/env bash
# Generates the demo HLS asset served by the local CDN-style origin (cdn-origin/).
# A 16-second invented test pattern is encoded once in two renditions (360p, 180p) with 4-second segments.
# The media playlists then reference those same four segments in a loop (with EXT-X-DISCONTINUITY)
# so the demo "film" runs for 20 minutes and a resume at 12:30 really works, while the committed
# media stays well under 2 MB. Re-run only if you change the pattern: tools/make-demo-media.sh
set -euo pipefail
ROOT=$(cd "$(dirname "$0")/.." && pwd)
OUT="$ROOT/cdn-origin/demo/testpattern"
SECONDS_SRC=16; SEG=4; LOOP_MINUTES=20
command -v ffmpeg >/dev/null || { echo "ffmpeg not found: the committed cdn-origin/ asset is used as-is"; exit 0; }
FONT=$(fc-match -f '%{file}' 'DejaVu Sans:bold' 2>/dev/null || true)
TEXT=""
if [ -n "$FONT" ] && [ -f "$FONT" ]; then
  TEXT=",drawtext=fontfile=$FONT:text='CINEWAVE TEST PATTERN':fontcolor=white:fontsize=h/14:x=(w-tw)/2:y=h*0.08:box=1:boxcolor=black@0.6:boxborderw=8,drawtext=fontfile=$FONT:text='%{pts\\:hms}':fontcolor=white:fontsize=h/12:x=(w-tw)/2:y=h*0.80:box=1:boxcolor=black@0.6:boxborderw=8"
fi
rm -rf "$OUT"; mkdir -p "$OUT"
encode() { # name height vbitrate
  local name=$1 h=$2 vb=$3
  mkdir -p "$OUT/$name"
  ffmpeg -hide_banner -loglevel error -y \
    -f lavfi -i "testsrc2=size=640x360:rate=25:duration=$SECONDS_SRC" \
    -f lavfi -i "sine=frequency=330:sample_rate=44100:duration=$SECONDS_SRC" \
    -vf "scale=-2:$h$TEXT" -c:v libx264 -profile:v main -preset veryfast -pix_fmt yuv420p \
    -b:v "$vb" -maxrate "$vb" -bufsize "$vb" -g $((25*SEG)) -keyint_min $((25*SEG)) -sc_threshold 0 \
    -c:a aac -b:a 32k -ac 1 \
    -f hls -hls_time $SEG -hls_playlist_type vod -hls_segment_filename "$OUT/$name/seg%02d.ts" "$OUT/$name/source.m3u8"
  # Build the looped VOD playlist from the segment list ffmpeg wrote.
  mapfile -t lines < <(grep -A1 '^#EXTINF' "$OUT/$name/source.m3u8" | grep -v '^--$')
  local loops=$(( LOOP_MINUTES * 60 / SECONDS_SRC ))
  {
    echo "#EXTM3U"; echo "#EXT-X-VERSION:3"; echo "#EXT-X-TARGETDURATION:$SEG"
    echo "#EXT-X-MEDIA-SEQUENCE:0"; echo "#EXT-X-PLAYLIST-TYPE:VOD"
    for ((i=0; i<loops; i++)); do
      [ $i -gt 0 ] && echo "#EXT-X-DISCONTINUITY"
      printf '%s\n' "${lines[@]}"
    done
    echo "#EXT-X-ENDLIST"
  } > "$OUT/$name/index.m3u8"
  rm "$OUT/$name/source.m3u8"
}
encode 360p 360 300k
encode 180p 180 110k
cat > "$OUT/master.m3u8" <<M3U
#EXTM3U
#EXT-X-VERSION:3
#EXT-X-STREAM-INF:BANDWIDTH=380000,AVERAGE-BANDWIDTH=340000,RESOLUTION=640x360,CODECS="avc1.4d401e,mp4a.40.2"
360p/index.m3u8
#EXT-X-STREAM-INF:BANDWIDTH=170000,AVERAGE-BANDWIDTH=150000,RESOLUTION=320x180,CODECS="avc1.4d400c,mp4a.40.2"
180p/index.m3u8
M3U
du -sh "$OUT"; find "$OUT" -type f | sort
