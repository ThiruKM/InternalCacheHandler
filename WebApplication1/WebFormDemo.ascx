<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WebFormDemo.ascx.cs" Inherits="WebApplication1.WebUserControl1" %>
<%@ OutputCache Duration="120" VaryByParam="none" %>
<p> Current Date and Time: </p><%= DateTime.Now.ToString()%>