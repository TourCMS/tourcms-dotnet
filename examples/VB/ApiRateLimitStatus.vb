Imports TourCMS.Utils
Imports System.Xml
Module Module1
    Sub Main()
        ' API Settings

        ' Set your Marketplace ID 
        ' For Suppliers this will be 0 
        ' For Partners this can be found in the API settings page 
        Dim marketplaceId As Integer = 0

        ' Set the Channel ID 
        ' For Suppliers this can be found in the API settings page 
        ' For Partners this will be 0 
        Dim channelId As Integer = 0

        ' Set your API priate Key, find this in the API settings page
        Dim privateKey As String = ""

        ' End API Settings

        ' Create a new TourCMS object
        Dim myTourCMS As New marketplaceWrapper(marketplaceId, privateKey)

        ' Call the API
        Dim doc As XmlDocument = myTourCMS.ApiRateLimitStatus(channelId)

        ' Get the status from the XML, will be "OK" unless there's a problem
        Dim status As String = doc.GetElementsByTagName("error")(0).InnerText


        If status = "OK" Then
            ' All ok, display API hits
            Dim limit As String = doc.GetElementsByTagName("remaining_hits")(0).InnerText
            Console.WriteLine("Remaining Hits: " & limit)
        Else
            ' There is a problem, display the error message
            Console.WriteLine("Error: " & status)
            Console.WriteLine("http://www.tourcms.com/support/api/mp/error_messages.php")
        End If
    End Sub
End Module
