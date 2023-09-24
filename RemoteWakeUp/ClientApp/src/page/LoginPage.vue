<script setup>
import {ref} from 'vue'
import {ElInput, ElButton, ElMessage} from 'element-plus'
import {Login} from "@/tool/httpRequest.js";
import {useGoToHome} from "@/router/goToRouter.js";
import {useI18n} from "vue-i18n";


const {t} = useI18n();

const goToHome = useGoToHome();

const password = ref('')
const submit = () => {
    if (password.value) {
        Login({
            password: password.value
        }, (data) => {
            ElMessage.success('登录成功');
            goToHome();
        }, (data) => {
            ElMessage.error(data);
        });
    } else {
        ElMessage.error('请输入密码');
    }
}
</script>

<template>
    <div id="login">
        <h1>{{ $t('loginPage.title') }}</h1>
        <el-input
            class="password_input"
            v-model="password"
            type="password"
            :placeholder="$t('loginPage.passwordPlaceholder')"
            show-password
            @keyup.enter="submit"
        />
        <el-button
            class="password-button"
            @click="submit"
            type="primary"
        >
            {{ $t('loginPage.loginButton') }}
        </el-button>
    </div>
</template>

<style scoped>

#login {
    width: 30%;
    height: 30%;
    margin: auto;
    backdrop-filter: blur(2px);
    border-radius: 10px;
    padding: 20px 50px 50px 50px;
    border: 1px solid rgba(0, 0, 0, 0.01);
    box-shadow: 0 0 10px rgba(0, 0, 0, 0.05);
}

.dark #login {
    border: 1px solid rgba(255, 255, 255, 0.01);
    box-shadow: 0 0 10px rgba(255, 255, 255, 0.03);
}

@media screen and (max-width: 768px) {
    #login {
        width: 80%;
        height: 80%;
    }
}

.password_input {
    margin-bottom: 15px;
}

.password-button {
    width: 100%;
}
</style>