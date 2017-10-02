// NETerror.cpp : implementation file
//

#include "stdafx.h"
#include "NETerror.h"
//#include "afxdialogex.h"


// NETerror dialog

IMPLEMENT_DYNAMIC(NETerror, CDialog)

	NETerror::NETerror(CWnd* pParent /*=NULL*/)
	: CDialog(NETerror::IDD, pParent)
{
	Create(IDD);
	DoModal();
}

NETerror::~NETerror()
{

}

void NETerror::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}


BEGIN_MESSAGE_MAP(NETerror, CDialog)
	ON_NOTIFY(NM_CLICK, IDC_SYSLINK1, &NETerror::OnNMClickSyslink1)
END_MESSAGE_MAP()


// NETerror message handlers


void NETerror::OnNMClickSyslink1(NMHDR *pNMHDR, LRESULT *pResult)
{
	// TODO: Add your control notification handler code here
	ShellExecuteA(NULL, "open", "http://www.microsoft.com/en-us/download/details.aspx?id=17851", NULL, NULL, 0);
	*pResult = 0;
}
