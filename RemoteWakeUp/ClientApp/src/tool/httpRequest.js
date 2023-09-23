//axios
import axios from 'axios';
import {getToken, removeToken, setToken} from "./operateLocalStorage.js";
import {ElLoading, ElMessage} from "element-plus";
import router from "@/router/index.js";

const config =
    {
        // "Url": window.location.origin,
        "Url": "http://localhost:9000",//本地测试使用 编译时请注释
        "Login": "/api/Command/login",
        "WakeUp": "/api/Command/wakeUp",
        "Test": "/api/Command/test",
    }

//登录
export function Login(data, ok, err) {
    PostHelp(config.Url + config.Login, data, (data) => {
        setToken(data.data.token);
        console.log(data);
        ok(data);
    }, (error) => {
        err(error);
    });
}

//验证token
export function Test() {
    GetHelp(config.Url + config.Test, (data) => {
        const isAuthenticated = !!getToken();
        if (!isAuthenticated) {
            setToken("No verification required")
            router.push("/");
        }
    }, (error) => {
    }, getToken());
}

//唤醒全部设备
export function WakeAllDevice(ok, err) {
    GetHelp(config.Url + config.WakeUp, (data) => {
        ok(data);
    }, (error) => {
        err(error);
    }, getToken());
}

//Get请求
export async function GetHelp(url, ok, err, token = '') {
    const loading = ElLoading.service({
        lock: true,
        text: 'Loading',
        background: 'rgba(0, 0, 0, 0.7)',
    })
    try {
        const response = await axios.get(url, {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + token
            }
        });

        if (response.status !== 200) {
            err && err(response.statusText);
        } else {
            if (response.data.code !== 0) {
                err && err(response.data.message);
                if (response.data.code === 1) {
                    removeToken();
                    router.push("/login");
                }
            } else {
                ok(response.data);
            }
        }
    } catch (error) {
        err && err(error.message);
    }
    loading.close();
}

//Post请求 ok返回json数据 err返回错误信息
export async function PostHelp(url, data, ok, err, token = '') {
    const loading = ElLoading.service({
        lock: true,
        text: 'Loading',
        background: 'rgba(0, 0, 0, 0.7)',
    })

    try {
        const response = await axios.post(url, data, {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + token
            }
        });

        if (response.status !== 200) {
            err && err(response.statusText);
        } else {
            if (response.data.code !== 0) {
                err && err(response.data.message);
                if (response.data.code === 1) {
                    removeToken();
                    router.push("/login");
                }
            } else {
                ok(response.data);
            }
        }
    } catch (error) {
        err && err(error.message);
    }

    loading.close();
}
