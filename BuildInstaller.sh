#!/bin/bash

# --- Configuration ---
VERSION="1.0.0"
ARCH="amd64"
APP_NAME="audiotools"
DEB_DIR="${APP_NAME}_${VERSION}_${ARCH}"
APPDIR="${APP_NAME}.AppDir"

echo "========================================="
echo " Building AudioTools v${VERSION}         "
echo "========================================="

# 1. Compile the .NET Application
echo "-> Publishing .NET Application..."
dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true
PUBLISH_DIR="bin/Release/net8.0/linux-x64/publish"


# =========================================
# 2. Build the .deb Package
# =========================================
echo "-> Preparing .deb package structure..."
mkdir -p ${DEB_DIR}/DEBIAN
mkdir -p ${DEB_DIR}/usr/bin
mkdir -p ${DEB_DIR}/usr/share/pixmaps
mkdir -p ${DEB_DIR}/usr/share/applications

echo "-> Copying files for .deb..."
cp ${PUBLISH_DIR}/AudioTools ${DEB_DIR}/usr/bin/audiotools
chmod 755 ${DEB_DIR}/usr/bin/audiotools
cp AudioTools_icon256.png ${DEB_DIR}/usr/share/pixmaps/audiotools.png

echo "-> Writing .deb Control file..."
cat <<EOF > ${DEB_DIR}/DEBIAN/control
Package: audiotools
Version: ${VERSION}
Architecture: ${ARCH}
Maintainer: Heroin-Bob
Depends: libc6, libgtk-3-0
Section: sound
Priority: optional
Description: AudioTools for Linux
 GUI wrapper for Yabridge and Linux Audio tools.
EOF

echo "-> Writing Desktop file..."
cat <<EOF > ${DEB_DIR}/usr/share/applications/audiotools.desktop
[Desktop Entry]
Version=1.0
Type=Application
Name=AudioTools
Comment=Manage Windows VSTs and Audio Settings
Exec=audiotools
Icon=audiotools
Terminal=false
Categories=Audio;AudioVideo;Utility;
EOF

echo "-> Building .deb package..."
dpkg-deb --build ${DEB_DIR}


# =========================================
# 3. Build the .AppImage
# =========================================
echo "-> Preparing AppDir for AppImage..."
mkdir -p ${APPDIR}/usr/bin
cp ${PUBLISH_DIR}/AudioTools ${APPDIR}/usr/bin/audiotools
chmod +x ${APPDIR}/usr/bin/audiotools

# AppImages require the icon and desktop file at the root of the AppDir
cp AudioTools_icon256.png ${APPDIR}/audiotools.png
cp ${DEB_DIR}/usr/share/applications/audiotools.desktop ${APPDIR}/audiotools.desktop

# Create the AppRun symlink (this tells the AppImage what to launch)
cd ${APPDIR}
ln -sf usr/bin/audiotools AppRun
cd ..

# Download appimagetool if it doesn't exist locally
if [ ! -f "appimagetool-x86_64.AppImage" ]; then
    echo "-> Downloading appimagetool..."
    wget -q https://github.com/AppImage/AppImageKit/releases/download/13/appimagetool-x86_64.AppImage
    chmod +x appimagetool-x86_64.AppImage
fi

echo "-> Building AppImage..."
# Extracting appimagetool prevents issues if the host system lacks FUSE
./appimagetool-x86_64.AppImage --appimage-extract-and-run ${APPDIR} AudioTools-v${VERSION}-x86_64.AppImage


# =========================================
# 4. Clean up
# =========================================
echo "-> Cleaning up staging directories..."
rm -r ${DEB_DIR}
rm -r ${APPDIR}

echo "========================================="
echo " Build Complete! "
echo " - Debian Package: ${DEB_DIR}.deb"
echo " - AppImage:       AudioTools-v${VERSION}-x86_64.AppImage"
echo "========================================="