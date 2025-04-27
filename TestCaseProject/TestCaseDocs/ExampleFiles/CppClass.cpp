public class Test
{
    #pragma region How to xyz
    void Sample3::HandleErrors(MyResult^ reply, String^ methodCode)
    {
        HandleValidationErrors(reply, methodCode);
        HandleMethodErrors(reply, methodCode);
    }
    #pragma endregion How to xyz
}
