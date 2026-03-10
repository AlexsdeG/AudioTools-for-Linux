#!/bin/bash

# --- Configuration ---
VERSION="0.99.0"
ARCH="amd64"
APP_NAME="audiotools"
DEB_DIR="${APP_NAME}_${VERSION}_${ARCH}"
BUILD_DIR="Build"
APPDIR="${APP_NAME}.AppDir"
OUTPUT_APPIMAGE="AudioTools-v${VERSION}-x86_64.AppImage"

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
deb_out="${DEB_DIR}.deb"
if dpkg-deb --build "${DEB_DIR}"; then
    mkdir -p "${BUILD_DIR}"
    mv "${deb_out}" "${BUILD_DIR}/"
    echo "-> .deb created: ${BUILD_DIR}/${deb_out}"
else
    echo "-> ERROR: dpkg-deb failed" >&2
fi


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

# Create a proper AppRun script (symlinks can sometimes fail in AppImages for .NET apps)
cat << 'EOF' > ${APPDIR}/AppRun
#!/bin/sh
HERE="$(dirname "$(readlink -f "${0}")")"
export PATH="${HERE}/usr/bin:${PATH}"
exec "${HERE}/usr/bin/audiotools" "$@"
EOF
chmod +x ${APPDIR}/AppRun

# Download appimagetool using curl (more reliable for GitHub redirects)
if [ ! -f "appimagetool-x86_64.AppImage" ]; then
    echo "-> Downloading appimagetool..."
    curl -L -o appimagetool-x86_64.AppImage https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage
    
    if [ ! -f "appimagetool-x86_64.AppImage" ] || [ ! -s "appimagetool-x86_64.AppImage" ]; then
        echo "-> ERROR: Failed to download appimagetool. Check your internet connection."
        exit 1
    fi
    chmod +x appimagetool-x86_64.AppImage
fi

echo "-> Building AppImage..."
# Using the extracted run method to prevent FUSE requirement issues during the build process
./appimagetool-x86_64.AppImage --appimage-extract-and-run ${APPDIR} "${OUTPUT_APPIMAGE}"

if [ -f "${OUTPUT_APPIMAGE}" ]; then
    mkdir -p "${BUILD_DIR}"
    mv "${OUTPUT_APPIMAGE}" "${BUILD_DIR}/"
else
    echo "-> ERROR: AppImage creation failed" >&2
fi

# =========================================
# 4. Clean up
# =========================================
echo "-> Cleaning up staging directories and build files..."
rm -rf "${DEB_DIR}"
rm -rf "${APPDIR}"
rm -f appimagetool-x86_64.AppImage
rm -rf squashfs-root

echo "========================================="
echo " Build Complete! "
echo " - Output directory: ${BUILD_DIR}/"
if [ -f "${BUILD_DIR}/${deb_out}" ]; then
    echo " - Debian Package: ${BUILD_DIR}/${deb_out}"
else
    echo " - Debian Package: not created"
fi
if [ -f "${BUILD_DIR}/${OUTPUT_APPIMAGE}" ]; then
    echo " - AppImage:       ${BUILD_DIR}/${OUTPUT_APPIMAGE}"
else
    echo " - AppImage:       not created"
fi
echo "========================================="