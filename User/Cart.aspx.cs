﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Foodienator.User
{
    public partial class Cart : System.Web.UI.Page
    {
        SqlConnection con;
        SqlCommand cmd;
        SqlDataAdapter sda;
        DataTable dt;
        decimal GrandTotal = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["UserId"]==null)
                {
                    Response.Redirect("Login.aspx");
                }
                else
                {
                    getCartItems();
                }
            }
        }
        void getCartItems()
        {
            con = new SqlConnection(Connection.GetConnectionString());
            cmd = new SqlCommand("Cart_Crud", con);
            cmd.Parameters.AddWithValue("@Action", "SELECT");
            cmd.Parameters.AddWithValue("@UserId", Session["UserId"]);
            cmd.CommandType = CommandType.StoredProcedure;
            sda = new SqlDataAdapter(cmd);
            dt = new DataTable();
            sda.Fill(dt);
            rCartItem.DataSource = dt;
            if (dt.Rows.Count == 0)
            {
                rCartItem.FooterTemplate = null;
                rCartItem.FooterTemplate = new CustomTemplate(ListItemType.Footer);
            }
            rCartItem.DataBind();
        }

        protected void rCartItem_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if(e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Label totalPrice = e.Item.FindControl("lblTotalPrice") as Label;
                Label productPrice = e.Item.FindControl("lblPrice") as Label;
                TextBox quantity = e.Item.FindControl("txtQuantity") as TextBox;
                decimal calTotalPrice = Convert.ToDecimal(productPrice.Text) * Convert.ToDecimal(quantity.Text);
                totalPrice.Text = calTotalPrice.ToString();
                GrandTotal += calTotalPrice;

            }
            Session["grandTotalPrice"] = GrandTotal;
        }

        protected void rCartItem_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            Utils utils = new Utils();
            if(e.CommandName == "remove")
            {
                con = new SqlConnection(Connection.GetConnectionString());
                cmd = new SqlCommand("Cart_Crud", con);
                cmd.Parameters.AddWithValue("@Action", "DELETE");
                cmd.Parameters.AddWithValue("@ProductId", e.CommandArgument);
                cmd.Parameters.AddWithValue("@UserId", Session["UserId"]);
                cmd.CommandType = CommandType.StoredProcedure;
            }
            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
                getCartItems();
                Session["cartCount"] = utils.cartConunt(Convert.ToInt32(Session["UserId"]));
            }
            catch (Exception ex)
            {
                System.Web.HttpContext.Current.Response.Write("<script>alert('Error - " + ex.Message + "')</script>");
            }
            finally
            {
                con.Close();
            }
            if(e.CommandName == "updateCart")
            {
                bool isCartUpdated = false;
                for(int item = 0;item < rCartItem.Items.Count; item++)
                {
                    if (rCartItem.Items[item].ItemType == ListItemType.Item || rCartItem.Items[item].ItemType == ListItemType.AlternatingItem)
                    {
                        TextBox quantity = rCartItem.Items[item].FindControl("txtQuantity") as TextBox;
                        HiddenField _productId = rCartItem.Items[item].FindControl("hdnProductId") as HiddenField;
                        HiddenField _quantity = rCartItem.Items[item].FindControl("hdnQuantity") as HiddenField;
                        int quantityFromCart = Convert.ToInt32(quantity.Text);
                        int ProductId = Convert.ToInt32(_productId.Value);
                        int QuantityFromDB = Convert.ToInt32(_quantity.Value);
                        bool isTrue = false;
                        int updateQuantity = 1;
                        if (quantityFromCart > QuantityFromDB) { 
                        updateQuantity = quantityFromCart;
                            isTrue = true;
                        }
                        else if (quantityFromCart < QuantityFromDB)
                        {
                            updateQuantity = quantityFromCart;
                            isTrue = true;
                        }
                        if (isTrue)
                        {
                            isCartUpdated = utils.UpdateCartQuantity(updateQuantity, ProductId, Convert.ToInt32(Session["UserId"]));
                        }
                    }
                }
                getCartItems();
            }
        }
        private sealed class CustomTemplate : ITemplate
        {
            private ListItemType ListItemType { get; set; }

            public CustomTemplate(ListItemType type)
            {
                ListItemType = type;
            }
            public void InstantiateIn(Control container)
            {
                var footer = new LiteralControl("<tr><td colspan ='5'><b>Your cart is empty.</b><a href = 'Menu.aspx' class = 'badge badge-info ml-2'>Continue Shopping</a></td></tr></tbody></table>");
                container.Controls.Add(footer);
                    }
        }
    }
}