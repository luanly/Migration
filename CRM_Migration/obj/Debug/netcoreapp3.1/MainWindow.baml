   M S B A M L   `   `   `  ����H  DCRM_Migration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:CRM_MigrationCRM_Migration  5   	       CRM_Migration.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:CRM_Migration     5   	     � Title$  
MainWindow��5   	   $	��450��6   $	��800��6)   .��5	      �  � ColumnDefinitions 5
   
   �� 5      �� 5      �� 5      $
��0.5* �6   �� 5      �� 5      �� 5      �� 5       � RowDefinitions 5   
   �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      .+�5   
   �� -   5        $��btn_Open��5      $��1��6!   $��1��61   $
��Open��6>   6M   �� 5   
   -   5         ��Name$ txt_Msg��5      $��1��6%   $��2��65   $	��Top=�6B   �� 5   
   -   5        $ txt_Msg2��5      $��1��6&   $��2��66   $��Bottom=�6C   �� 5   
   -   5        $��btn_Migrate��5   #   ${�False��6   $��1��66   $��3��6F   $��Migrate��6S   6e   �� 5    
   -   5         ��Name$ txtB_Log��5       $��3��6$   $��1��64   $��3��6A   $��4��6U    �� TextWrapping$
 Wrap=�6f    �� AcceptsReturn$
 True��6z   $ �� HorizontalScrollBarVisibility$ Disabled=�6�   " �� VerticalScrollBarVisibility$
 Auto=�6�   �� 5!   
   -   5        $ txt_Progress��5!      $��5��6*   $��3��67   $��3��6G   $��Center=�6[   5"      5#      