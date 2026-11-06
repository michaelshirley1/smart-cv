#!/usr/bin/env bash
set -euo pipefail

runtime="${1:-linux-x64}"
cd "$(dirname "$0")"

npm ci
npm run build
dotnet publish server/SmartCv.csproj -c Release -r "$runtime" --self-contained -p:PublishSingleFile=true -o "publish/$runtime"

if [[ "$runtime" == osx-* ]]; then
    app="publish/$runtime/SmartCv.app"
    rm -rf "$app"
    mkdir -p "$app/Contents/MacOS"
    cp "publish/$runtime/SmartCv" "$app/Contents/MacOS/SmartCv"
    cat > "$app/Contents/Info.plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>Smart CV</string>
    <key>CFBundleIdentifier</key>
    <string>app.smartcv</string>
    <key>CFBundleExecutable</key>
    <string>SmartCv</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>0.1.0</string>
</dict>
</plist>
PLIST
fi

echo "Built publish/$runtime"
