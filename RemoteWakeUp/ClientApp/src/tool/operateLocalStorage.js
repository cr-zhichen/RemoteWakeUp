import {globalState} from "../global/globalState.js";

//将Token存储到本地
export function setToken(token) {
    localStorage.setItem("token", token);
}

//从本地存储获取Token
export function getToken() {
    let token = localStorage.getItem("token");
    if (!token) {
        return null;
    } else {
        return token;
    }
}

//从本地删除Token
export function removeToken() {
    localStorage.removeItem("token");
}

//将RecaptchaClientKey存储到本地
export function setRecaptchaClientKey(RecaptchaClientKey) {
    localStorage.setItem("RecaptchaClientKey", RecaptchaClientKey);
}

//从本地存储获取RecaptchaClientKey
export function getRecaptchaClientKey() {
    let RecaptchaClientKey = localStorage.getItem("RecaptchaClientKey");
    if (!RecaptchaClientKey) {
        return null;
    } else {
        return RecaptchaClientKey;
    }
}

//从本地删除RecaptchaClientKey
export function removeRecaptchaClientKey() {
    localStorage.removeItem("RecaptchaClientKey");
}