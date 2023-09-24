<script setup>
import {useGoToLogin} from "@/router/goToRouter.js";
import {ref} from "@vue/reactivity";
import {getThemeIcon, toggleTheme} from "@/tool/themeChange.js";
import {getLanguageName, toggleLanguage} from "@/tool/languageChange.js";
import {removeToken} from "@/tool/operateLocalStorage.js";
import {WakeAllDevice} from "@/tool/httpRequest.js";
import {ElMessage} from "element-plus";

const theme = ref(getThemeIcon());
const swatchTheme = () => {
    toggleTheme();
    theme.value = getThemeIcon();
}

const language = ref(getLanguageName());
const switchLanguage = () => {
    toggleLanguage();
    language.value = getLanguageName();
}

const goToLogin = useGoToLogin();

// 退出登录
const logout = () => {
    removeToken();
    goToLogin();
}

//远程唤醒全部设备
const wakeAll = () => {
    WakeAllDevice({}, (data) => {
        ElMessage.success('唤醒成功');
    }, (data) => {
        ElMessage.error(data);
    });
}

</script>

<template>
    <div id="homePage">
        <h1>{{ $t('homePage.title') }}</h1>
        <!--        <el-button @click="swatchTheme" :icon="theme">{{ $t('homePage.swatchThemeButton') }}</el-button>-->
        <!--        <el-button @click="switchLanguage">{{ language }}</el-button>-->
        <el-button @click="wakeAll">{{ $t('homePage.wakeAllButton') }}</el-button>
        <el-button @click="logout">{{ $t('homePage.logoutButton') }}</el-button>
    </div>
</template>

<style scoped>

</style>