// 导入router所需的方法
import {createRouter, createWebHashHistory, createWebHistory} from 'vue-router'

// 导入路由页面的配置
import routes from './routes'
import {getToken} from "@/tool/operateLocalStorage.js";

// 路由参数配置
const router = createRouter({
    // 使用hash(createWebHashHistory)模式，(createWebHistory是HTML5历史模式，支持SEO)
    history: createWebHashHistory(),
    routes: routes,
})

// 全局前置守卫，这里可以加入用户登录判断
router.beforeEach((to, from, next) => {
    const isAuthenticated = !!getToken(); // 如果用户已认证，isAuthenticated 为 true，否则为 false。
    if (to.name !== 'login' && !isAuthenticated) {
        // 如果用户未认证，并且目标路由不是登录页，重定向到登录页。
        next({name: 'login'});
    } else if (to.name === 'login' && isAuthenticated) {
        // 如果用户已认证，并且目标路由是登录页，重定向到主页。
        next({path: '/'});
    } else {
        // 在所有其他情况下，允许用户访问目标路由。
        next();
    }
});

// 全局后置钩子，这里可以加入改变页面标题等操作
router.afterEach((to, from) => {
    const _title = to.meta.title
    if (_title) {
        window.document.title = _title
    }
})

// 导出默认值
export default router
