   M S B A M L   `   `   `  ����D  @ClientPDS, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:ClientPDS	ClientPDS  5          ClientPDS.ProcessesView   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      )localclr-namespace:ClientPDS     5      U QMahApps.Metro, Version=1.5.0.23, Culture=neutral, PublicKeyToken=f4fb5a3c4d1e5b4f;Controls,http://metro.mahapps.com/winfx/xaml/controls  5      .��5      �   � RowDefinitions  5   
   �� 5      �� 5      $��30 �6   .+�5   
   &   System.Windows.Controls.DataGrid  -   5        $
��Apps��5      $��0��6H      AutoGenerateColumns .     SelectionMode$ Extended=�5   g      SelectionUnit$ FullRow=�6�      
IsReadOnly . $��0,0,0,30q�5   %   ��5       ��  �� Path$ 	Processes� �� SelectedItem 5      �� $ FocusedProcess� �� UpdateSourceTrigger$ PropertyChanged=� � 	Resources 5         MetroDataGridColumnHeader	 �� BasedOn=  7System.Windows.Controls.Primitives.DataGridColumnHeader
 �� 
TargetType+  %MahApps.Metro.Controls.ControlsHelper �� Property �� Value��%v   '        7   �� 5      8	   "
  6M   .�5      ��  �@ ContentCharacterCasing$ Normal=�6W   5      5         	CellStyle 5      �� 5      *  $System.Windows.Controls.DataGridCell"
  6   .�5      ��  �@J�IsHitTestVisible .  6I   �� 5        � ��$ 0,5,0,0q�6/   6A    �� 	Resources 5#        WindowGlowBrush �� Color��%�   (��       5$   *   *1/SystemColors.InactiveSelectionHighlightBrushKey+)(��      5%   *   *53SystemColors.InactiveSelectionHighlightTextBrushKey+)�� 5$      # �  6m   �� 5%      $ White��6q   5&      5(         Columns 5.      4  .System.Windows.Controls.DataGridTemplateColumn  5/      ,  &System.Windows.Controls.DataGridColumn   Header$ App Name��6*      CellTemplate 50      �� 51      .R�52      ��  �� Orientation$ 
Horizontal=�6)   .+�53   "   �� $��24��6E   $��0,0,5,0q�6P   ��6(   �� $
 Icon�6P   �� 54   "   $��Right=�6I   $��0,-5,0,0q�6e   ��6)   �� $ Title�6e   55      57      59      0  *System.Windows.Controls.DataGridTextColumn  5;      $	 Pid��6%   1  +System.Windows.Controls.DataGridBoundColumn   Binding 62   �� $	 Pid�  5=      $
 Path��6%    63   �� $
 Path�  5?      $ 
Percentage��6%    69   �� $ FocusPercentage� �� StringFormat$
 {0}%��5B      )	  #MahApps.Metro.Controls.ProgressRing	  5D   
   #���  6n    	  IsActive 6    �� $ ProgressRing�$ PropertyChanged=�6n   � 5F   
   $��1��6   .+�5G      �� $ 
Horizontal=�6   $
��Left=�62   .+�5J      �� $ 
Horizontal=�6   $��Center=�68   .+�5M      )
  #MahApps.Metro.Controls.ToggleSwitch
  -   5        $��EnableHotkeySwitch��5M   ,    + 'MahApps.Metro.Styles.ToggleSwitch.Win10#��[ 6F    
  OffSwitchBrush
 �����$��Enable Hot Key��5O   $   $��12(�6=   $��0,0,10,0q�6K   {�5N   @   �� $ ShortcutToggleEnabled�5O   K   
  5Q      -   5        $��	altSwitch��5Q   ,   #��[ 6=   
 �����$	��ALT��5S   $   $��12(�62    J� IsHitTestVisible .  $��2,0,2,0q�6Y   
  5T      -   5        $��
ctrlSwitch��5T   ,   #��[ 6>   
 �����$
��CTRL��5V   $   $��12(�63    .  $��2,0,2,0q�6Z   
  5W      -   5        $��shiftSwitch��5W   ,   #��[ 6?   
 �����$��SHIFT��5Y   $   $��12(�64    .  $��2,0,2,0q�6[   5Z      5]      �� 5`      $ 
Horizontal=�6   $��Right=�62   .+�5a      �� -   5         ��Name$ ServerIpLbl��5a      $��Insert server IP: ��6-   $��Right=�6J   $��Center=�6m   �� 5b      -   5         ��Name$ ServerIpTxt��5b       �� 	MaxLength$ 15��6/    �� TextWrapping$
 Wrap=�6>   $	��100��6S   $��Center=�6�   $��Center=�6�   $��0,0,10,0q�6�   ��6_   �� $ ServerIP�{�6~   �� $ IpTextEnabled�6�   �� 5c      -   5         ��Name$ ConnectionBtn��5c      $��30��6T   $��80��6c   $��Right=�6n   $��Center=�6�   ��60   �� $ 
ButtonText�6�   5d      5e      5h      5k      