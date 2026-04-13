#!/bin/bash

# 设置变量

BASE_DIR="/opt/NXP/S32DS.3.6.6"
RTD_VER="S32K3_7.0.1"

# Release 不可更改脚本

SCRIPT_PATH="$0"
if [[ -f "$SCRIPT_PATH" ]]; then
    SCRIPT_DIR="$(dirname "$(readlink -f "$SCRIPT_PATH")")"
else
    SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"
fi

LOWER_DIR="${BASE_DIR}/S32DS"
UPPER_DIR="${BASE_DIR}/${RTD_VER}"
MOUNT_POINT="/opt/NXP/S32DS"
TMP_POINT="/tmp/S32DS"

echo RTD_VER is ${RTD_VER}
echo BASE_DIR is ${BASE_DIR}
echo LOWER_DIR is ${LOWER_DIR}
echo UPPER_DIR is ${UPPER_DIR}

# 创建挂载点目录（如果不存在）
mkdir -p "$MOUNT_POINT"
mkdir -p "$TMP_POINT/upper"
mkdir -p "$TMP_POINT/work"

# 检查是否已经挂载
if mountpoint -q "$MOUNT_POINT"; then
    sudo umount "$MOUNT_POINT"
fi

# 使用overlayfs进行挂载
sudo mount -t overlay overlay \
    -o lowerdir="$UPPER_DIR/upper:$LOWER_DIR",upperdir="$TMP_POINT/upper",workdir="$TMP_POINT/work" \
    "$MOUNT_POINT"

if [ $? -ne 0 ]; then
    echo "错误: 挂载失败"
    exit 1
fi

echo "挂载成功: $MOUNT_POINT"

# 执行s32ds.sh，并传递所有输入参数
"$MOUNT_POINT/s32ds.sh" -data "${SCRIPT_DIR}"

sleep 2

# 卸载挂载点
sudo umount "$MOUNT_POINT"
sudo rm -rf "$TMP_POINT"
