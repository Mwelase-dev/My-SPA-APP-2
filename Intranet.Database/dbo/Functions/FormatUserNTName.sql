CREATE Function [dbo].[FormatUserNTName] (@UserName Varchar(100), @UserSurname Varchar(100)) Returns Varchar(100)
as
  Begin
    Declare @initial Varchar(1),    
            @surname Varchar(100),
            @count   Int;
            
    Select @initial = SubString(@UserName, 1, 1);
        
    Set @count   = 1;
    Set @surname = '';
    While @Count <= LEN(@UserSurname)
      Begin
        If SubString(@UserSurname, @Count, 1) <> ' '
        Set @surname = @surname + isNull(SubString(@UserSurname, @Count, 1), '');    
        Set @Count  = @Count + 1;
      end
    
    Return lower(@initial + RTrim(LTrim(@surname)));
  end    