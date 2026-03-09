#!/bin/bash

# --- Configuration ---
VERSION="1.0.0"
ARCH="amd64"
APP_NAME="audiotools"
DEB_DIR="${APP_NAME}_${VERSION}_${ARCH}"
BUILD_DIR="Build"
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
mkdir -p "${APPDIR}/usr/bin"
cp "${PUBLISH_DIR}/AudioTools" "${APPDIR}/usr/bin/audiotools"
chmod +x "${APPDIR}/usr/bin/audiotools"

# AppImages require the icon and desktop file at the root of the AppDir
cp AudioTools_icon256.png "${APPDIR}/audiotools.png"
cp "${DEB_DIR}/usr/share/applications/audiotools.desktop" "${APPDIR}/audiotools.desktop"

# Create the AppRun symlink (this tells the AppImage what to launch)
cd "${APPDIR}" || exit 1
ln -sf usr/bin/audiotools AppRun
cd ..

# Download appimagetool reliably
APPIMAGETOOL="appimagetool-x86_64.AppImage"
OUTPUT_APPIMAGE="AudioTools-v${VERSION}-x86_64.AppImage"

# Remove any stale appimagetool to force fresh download
if [ -f "${APPIMAGETOOL}" ]; then
    echo "-> Removing stale ${APPIMAGETOOL}"
    rm -f "${APPIMAGETOOL}"
fi

echo "-> Downloading appimagetool..."
DOWNLOAD_OK=0
if wget -q -O "${APPIMAGETOOL}" "https://github.com/AppImage/AppImageKit/releases/download/13/appimagetool-x86_64.AppImage"; then
    chmod +x "${APPIMAGETOOL}" || true
    echo "-> appimagetool downloaded"
    DOWNLOAD_OK=1
else
    echo "-> ERROR: Failed to download appimagetool." >&2
    # remove any partial file
    if [ -f "${APPIMAGETOOL}" ]; then rm -f "${APPIMAGETOOL}"; fi
fi

echo "-> Building AppImage..."
if [ "${DOWNLOAD_OK}" -eq 1 ] && [ -x "${APPIMAGETOOL}" ]; then
    # Extract-and-run to create the AppImage
    if ./${APPIMAGETOOL} --appimage-extract-and-run "${APPDIR}" "${OUTPUT_APPIMAGE}"; then
        if [ -f "${OUTPUT_APPIMAGE}" ]; then
            mkdir -p "${BUILD_DIR}"
            mv "${OUTPUT_APPIMAGE}" "${BUILD_DIR}/"
            echo "-> AppImage created: ${BUILD_DIR}/${OUTPUT_APPIMAGE}"
        else
            echo "-> ERROR: appimagetool reported success but output AppImage not found." >&2
        fi
    else
        echo "-> ERROR: appimagetool failed to build AppImage." >&2
    fi
else
    echo "-> Skipping AppImage: appimagetool not available or download failed." >&2
fi


# =========================================
# 4. Clean up
# =========================================
echo "-> Cleaning up staging directories..."
rm -rf "${DEB_DIR}"
rm -rf "${APPDIR}"

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