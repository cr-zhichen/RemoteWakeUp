<script setup>
import {useGoToLogin} from "@/router/goToRouter.js";
import {removeToken} from "@/tool/operateLocalStorage.js";
import {GetDevices, IsOnline, WakeAllDevice, WakeUpDevice} from "@/tool/httpRequest.js";
import {ElMessage} from "element-plus";
import {ref} from "vue";
import {useI18n} from "vue-i18n";

const {t} = useI18n();

const goToLogin = useGoToLogin();
const tableData = ref([]);

// 退出登录
const logout = () => {
    removeToken();
    goToLogin();
}

//远程唤醒全部设备
const wakeAll = () => {
    WakeAllDevice((data) => {
        ElMessage.success('唤醒成功');
    }, (data) => {
        ElMessage.error(data);
    });
}

//点击唤醒按钮
const wakeClick = (mac) => {
    WakeUpDevice({
        macList: [mac]
    }, (data) => {
        ElMessage.success('唤醒成功');
    }, (data) => {
        ElMessage.error(data);
    });
}

//刷新全部设备状态
const refreshAll = () => {
    for (let i = 0; i < tableData.value.length; i++) {
        tableData.value[i].state = t('homePage.table.obtaining');
        IsOnline(tableData.value[i].ip, (data2) => {
            //根据ip查找对应的行
            for (let j = 0; j < tableData.value.length; j++) {
                if (tableData.value[j].ip === tableData.value[i].ip) {
                    tableData.value[j].state = data2.data ? t('homePage.table.online') : t('homePage.table.offline');
                    break;
                }
            }
        }, (data) => {
            ElMessage.error(data);
        });
    }
}


GetDevices((data) => {
    for (let i = 0; i < data.data.length; i++) {
        tableData.value.push({
            name: data.data[i].name,
            ip: data.data[i].ip,
            mac: data.data[i].mac,
            state: t('homePage.table.obtaining')
        });
        IsOnline(data.data[i].ip, (data2) => {
            //根据ip查找对应的行
            for (let j = 0; j < tableData.value.length; j++) {
                if (tableData.value[j].ip === data.data[i].ip) {
                    tableData.value[j].state = data2.data ? t('homePage.table.online') : t('homePage.table.offline');
                    break;
                }
            }
        }, (data) => {
            ElMessage.error(data);
        });
    }
}, (data) => {
    ElMessage.error(data);
});

</script>

<template>
    <div id="homePage">
        <h1>{{ $t('homePage.title') }}</h1>
    </div>

    <div id="homePageTable">
        <el-table :data="tableData">
            <el-table-column fixed prop="name" :label="$t('homePage.table.name')" min-width="80px"/>
            <el-table-column prop="ip" :label="$t('homePage.table.ip')" min-width="120px"/>
            <el-table-column prop="mac" :label="$t('homePage.table.mac')" min-width="150px"/>
            <el-table-column prop="state" :label="$t('homePage.table.status')" min-width="80px"/>
            <el-table-column fixed="right" :label="$t('homePage.table.action')">
                <template #default="scope">
                    <el-button plain type="success" size="small" @click="wakeClick(scope.row.mac)">
                        {{ $t('homePage.table.wake') }}
                    </el-button>
                </template>
            </el-table-column>
        </el-table>
    </div>

    <div id="homePageButton">
        <el-button plain type="success" @click="wakeAll">{{ $t('homePage.wakeAllButton') }}</el-button>
        <el-button plain type="primary" @click="refreshAll">{{ $t('homePage.refreshAllButton') }}</el-button>
        <el-button plain type="info" @click="logout">{{ $t('homePage.logoutButton') }}</el-button>
    </div>
</template>

<style scoped>

#homePageTable {
    margin-top: 20px;
    margin-left: auto;
    margin-right: auto;
    max-width: 50%
}

@media screen and (max-width: 768px) {
    #homePageTable {
        max-width: 100%;
    }
}

#homePageButton {
    margin-top: 20px;
}

.el-table {
    border-radius: 10px;
    overflow: hidden;
}

</style>