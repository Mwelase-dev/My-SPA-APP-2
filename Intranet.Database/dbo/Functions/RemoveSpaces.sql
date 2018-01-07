Create Function [dbo].[RemoveSpaces] (@InString Varchar(500)) RETURNS Varchar(500)
AS
BEGIN
  Declare @TmpStr  Varchar(50),
          @TmpStr2 Varchar(50),
          @Count   Int;

  Set @Count  = 1;
  Set @TmpStr = '';
  While @Count <= LEN(@InString)
    Begin
      If SubString(@InString, @Count, 1) <> ' '
      Set @TmpStr = @TmpStr + isNull(SubString(@InString, @Count, 1), '');    
      Set @Count  = @Count + 1;
    end
    
  Set @Count   = 1;
  Set @TmpStr2 = '';
  While @Count <= LEN(@TmpStr)
    Begin
      If SubString(@TmpStr, @Count, 1) <> '-'
      Set @TmpStr2 = @TmpStr2 + isNull(SubString(@TmpStr, @Count, 1), '');    
      Set @Count  = @Count + 1;
    end    
    
  RETURN RTrim(LTrim(@TmpStr2));
END


