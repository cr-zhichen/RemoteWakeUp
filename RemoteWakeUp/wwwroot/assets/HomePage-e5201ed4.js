/* empty css                  */import{u as _,E as d}from"./goToRouter-18873fc1.js";import{s as h,m as f,a as p,r as a,g as T,o as k,c as A,b as B,t as s,d as n,w as r,e as E,f as c}from"./index-3da38d03.js";import{W as L,E as u}from"./httpRequest-1e2a487b.js";const t={LIGHT:"light",DARK:"dark",AUTO:"auto"};function v(){return localStorage.getItem("vueuse-color-scheme")}function w(){switch(v()){case t.LIGHT:return p;case t.DARK:return f;case t.AUTO:return h}}const D={id:"homePage"},H={__name:"HomePage",setup(l){a(w()),a(T());const m=_(),i=()=>{E(),m()},g=()=>{L(e=>{u.success("唤醒成功")},e=>{u.error(e)})};return(e,I)=>{const o=d;return k(),A("div",D,[B("h1",null,s(e.$t("homePage.title")),1),n(o,{onClick:g},{default:r(()=>[c("全部唤醒")]),_:1}),n(o,{onClick:i},{default:r(()=>[c(s(e.$t("homePage.logoutButton")),1)]),_:1})])}}};export{H as default};