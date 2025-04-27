Public Class Class1
    Private flag As Boolean

	Public Property FlagProp As Boolean
        Get
			Return flag
        End Get
        Set(ByVal value As Boolean)
            flag = value
        End Set
    End Property

#If DEBUG
    Public Function Test() As String
        Return "Test 1"
    End Function
#Else If DEBUG1
    Public Function Test() As String
        Return "Test 2"
    End Function
#ElseIf DEBUG2
    Public Function Test() As String
        Return "Test 3"
    End Function
#Else
    Public Function Test() As String
        Return "Test 4"
    End Function
#End If

    #Region "Test region"
    ''' <summary>
    ''' See <see cref="IndexTest" />
    ''' </summary>
    ''' <remarks>Die Fehlermeldung an den ScriptManager übergeben</remarks>
    Public Function TestMethod() As String

		' #Region "Embedded snippet"
        If flag = True Then
			Return "Test 3"
        End If

        Return "Test 2"
        ' #End Region

    End Function
    #End Region
End Class
